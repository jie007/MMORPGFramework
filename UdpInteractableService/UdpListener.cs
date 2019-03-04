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

        public ConcurrentDictionary<long, string> ConnectionIdToUserId = new ConcurrentDictionary<long, string>();
        public ConcurrentDictionary<string, string> Map = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<long, DateTime> UpdatesReceived = new ConcurrentDictionary<long, DateTime>();


        public void Update()
        {
            UdpManager.PollEvents();
            var peers = UdpManager.GetPeers();

            foreach (UdpPeer peer in peers)
            {
                var closurePeer = peer;
                if (!this.ConnectionIdToUserId.ContainsKey(closurePeer.ConnectId))
                    continue;

                string name = ConnectionIdToUserId[closurePeer.ConnectId];
                string map = Map[name];

                // TODO: Batch calls to same map
                if (UpdatesReceived.ContainsKey(closurePeer.ConnectId))
                {
                    ActorProxy.Create<IInteractableActorService>(new ActorId(map))
                        .GetUpdateStatus(UpdatesReceived[closurePeer.ConnectId])
                        .ContinueWith(x => SendStatus(x.Result, closurePeer));
                    UpdatesReceived[closurePeer.ConnectId] = DateTime.UtcNow;
                }
                else
                {
                    ActorProxy.Create<IInteractableActorService>(new ActorId(map))
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
            if (ConnectionIdToUserId.ContainsKey(peer.ConnectId))
            {
                string value = string.Empty;
                string name = ConnectionIdToUserId[peer.ConnectId];
                ConnectionIdToUserId.TryRemove(peer.ConnectId, out value);

                string map = string.Empty;
                Map.TryRemove(name, out map);
            }
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            try
            {
                if (ConnectionIdToUserId.ContainsKey(peer.ConnectId))
                {
                    string name = ConnectionIdToUserId[peer.ConnectId];
                    string map = Map[name];
                    switch (reader.PeekByte())
                    {
                        case (byte)MessageTypes.InteractableStart:
                            var msgStart = new StartInteractMessage(reader);
                            msgStart.ServerTimeStamp = DateTime.UtcNow.Ticks;
                            ActorProxy.Create<IInteractableActorService>(new ActorId(map)).StartInteraction(name, msgStart);
                            break;
                        case (byte)MessageTypes.InteractableFinish:
                            var msgFinish = new FinishInteractMessage(reader);
                            msgFinish.ServerTimeStamp = DateTime.UtcNow.Ticks;
                            ActorProxy.Create<IInteractableActorService>(new ActorId(map)).FinishInteraction(name, msgFinish);
                            break;
                    }
                }
                else
                {
                    var tokenMsg = new TokenMessage(reader);
                    string token = tokenMsg.Token;
                    string id = JwtTokenHelper.GetTokenClaim(token, "CharacterName");
                    string map = JwtTokenHelper.GetTokenClaim(token, "Map");

                    if (string.IsNullOrEmpty(id))
                    {
                        UdpManager.DisconnectPeer(peer);
                    }
                    else
                    {
                        ConnectionIdToUserId.AddOrUpdate(peer.ConnectId, (connId) => { return id; }, (connId, oldVal) => { return id; });

                        Map.AddOrUpdate(id, (connId) => { return map; }, (connId, oldVal) => { return map; });

                        UdpDataWriter writer = new UdpDataWriter();
                        tokenMsg.Write(writer);
                        peer.Send(writer, ChannelType.ReliableOrdered);
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
