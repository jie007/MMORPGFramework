using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Protocols;
using Common.Protocols.Interactable;
using Common.Protocols.Map;
using CommonServer;
using CommonServer.MapPartitioning;
using CommonServer.ServiceFabric;
using CommonServer.UdpServiceHelper;
using InteractableActorService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace UdpInteractableService
{
    public class UdpListener : IUdpListener
    {
        public UdpManager UdpManager { get; set; }

        public UdpConnectionManagment UdpConnectionManagment = new UdpConnectionManagment();
        public ConcurrentDictionary<long, DateTime> UpdatesReceived = new ConcurrentDictionary<long, DateTime>();

        public void Update()
        {
            UdpManager.PollEvents();
            var peers = UdpManager.GetPeers();

            foreach (UdpPeer peer in peers)
            {
                var closurePeer = peer;
                var user = UdpConnectionManagment.GetUser(closurePeer.ConnectId);
                if (user == null)
                    continue;

                // TODO: Batch calls to same map
                if (UpdatesReceived.ContainsKey(closurePeer.ConnectId))
                {
                    ActorProxy.Create<IInteractableActorService>(new ActorId(user.Map))
                        .GetUpdateStatus(UpdatesReceived[closurePeer.ConnectId])
                        .ContinueWith(x => SendStatus(x.Result, closurePeer));
                    UpdatesReceived[closurePeer.ConnectId] = DateTime.UtcNow;
                }
                else
                {
                    ActorProxy.Create<IInteractableActorService>(new ActorId(user.Map))
                        .GetAllStatus()
                        .ContinueWith(x => SendStatus(x.Result, closurePeer));
                    UpdatesReceived.AddOrUpdate(closurePeer.ConnectId, DateTime.UtcNow, (l, time) => DateTime.UtcNow);
                }
            }
        }

        private void SendStatus(List<InteractableStatusMessage> msgs, UdpPeer peer)
        {
            if (msgs.Count == 0)
                return;

            UdpDataWriter writer = new UdpDataWriter();
            foreach (var msg in msgs)
            {
                msg.Write(writer);
            }

            peer.Send(writer, ChannelType.ReliableOrdered);
        }

        public void OnPeerConnected(UdpPeer peer)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            var user = this.UdpConnectionManagment.RemoveUser(peer.ConnectId);
            if (user == null)
                return;

            DateTime removed;
            UpdatesReceived.TryRemove(peer.ConnectId, out removed);
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            try
            {
                var user = UdpConnectionManagment.GetUser(peer.ConnectId);
                if (user != null)
                {
                    switch (reader.PeekByte())
                    {
                        case (byte)MessageTypes.InteractableStart:
                            var msgStart = new StartInteractMessage(reader);
                            msgStart.ServerTimeStamp = DateTime.UtcNow.Ticks;
                            ActorProxy.Create<IInteractableActorService>(new ActorId(user.Map)).StartInteraction(user.Name, msgStart);
                            break;
                        case (byte)MessageTypes.InteractableFinish:
                            var msgFinish = new FinishInteractMessage(reader);
                            msgFinish.ServerTimeStamp = DateTime.UtcNow.Ticks;
                            ActorProxy.Create<IInteractableActorService>(new ActorId(user.Map)).FinishInteraction(user.Name, msgFinish);
                            break;
                    }
                }
                else
                {
                    var tokenMsg = new TokenMessage(reader);
                    user = UdpConnectionManagment.AddUser(peer.ConnectId, tokenMsg);
                    if (user != null)
                    {
                        UdpDataWriter writer = new UdpDataWriter();
                        tokenMsg.Write(writer);
                        peer.Send(writer, ChannelType.ReliableOrdered);
                    }
                    else
                    {
                        UdpManager.DisconnectPeer(peer);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnNetworkReceiveAck(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
        }

        public void OnNetworkReceiveUnconnected(UdpEndPoint remoteEndPoint, UdpDataReader reader)
        {
        }

        public void OnNetworkLatencyUpdate(UdpPeer peer, int latency)
        {
        }
    }
}
