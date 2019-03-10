using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatActorService.Interfaces;
using Common.Protocols;
using Common.Protocols.Chat;
using CommonServer;
using CommonServer.ServiceFabric;
using CommonServer.UdpServiceHelper;
using MapActorService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace UdpChatService
{
    public class UdpListener : IUdpListener
    {
        public UdpManager UdpManager { get; set; }
        public UdpConnectionManagment UdpConnectionManagment = new UdpConnectionManagment();

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

                IChatActor chatActor = ActorProxy.Create<IChatActor>(new ActorId(user.Name));
                chatActor.GetMessages().ContinueWith((taskMessages) =>
                {
                    var msgs = taskMessages.Result;
                    if (msgs.Count == 0)
                        return;

                    UdpDataWriter writer = new UdpDataWriter();
                    foreach (var msg in msgs)
                    {
                        var chatMsg = new ChatMessage()
                        {
                            Scope = msg.Scope,
                            FromOrTo = msg.Prefix,
                            Message = msg.Message
                        };
                        chatMsg.Write(writer);
                    }

                    closurePeer.Send(writer, ChannelType.ReliableOrdered);
                });

            }
        }

        public void OnPeerConnected(UdpPeer peer)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            UdpConnectionManagment.RemoveUser(peer.ConnectId);
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
                    var msg = new ChatMessage(reader);
                    string to = msg.FromOrTo;
                    msg.FromOrTo = user.Name;

                    switch (msg.Scope)
                    {
                        case ChatScope.Whisper:
                            var aMsg = new ActorChatMessage()
                            {
                                Message = msg.Message,
                                Prefix = user.Name,
                                Scope = msg.Scope
                            };

                            ActorProxy.Create<IChatActor>(new ActorId(user.Name)).WriteMessage(aMsg);
                            ActorProxy.Create<IChatActor>(new ActorId(to)).WriteMessage(aMsg);
                            break;
                        case ChatScope.Map:
                            ActorProxy.Create<IMapActorService>(new ActorId(user.Map)).SendChatMessage(msg).Wait();
                            break;
                    }
                }
                else
                {
                    var tokenMsg = new TokenMessage(reader);
                    user = UdpConnectionManagment.AddUser(peer.ConnectId, tokenMsg);
                    if (user == null)
                    {
                        UdpManager.DisconnectPeer(peer);
                    }
                    else
                    {
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
