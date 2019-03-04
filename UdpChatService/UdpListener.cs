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

        public ConcurrentDictionary<long, string> ConnectionIdToUserId = new ConcurrentDictionary<long, string>();
        public ConcurrentDictionary<string, string> Map = new ConcurrentDictionary<string, string>();

        public void Update()
        {
            UdpManager.PollEvents();
            var peers = UdpManager.GetPeers();

            foreach (UdpPeer peer in peers)
            {
                var closurePeer = peer;
                if (!this.ConnectionIdToUserId.ContainsKey(closurePeer.ConnectId))
                    continue;

                string id = this.ConnectionIdToUserId[closurePeer.ConnectId];
                IChatActor chatActor = ActorProxy.Create<IChatActor>(new ActorId(id));
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
            if (ConnectionIdToUserId.ContainsKey(peer.ConnectId))
            {
                var actor = ActorProxy.Create<IChatActor>(new ActorId(ConnectionIdToUserId[peer.ConnectId]));
                actor.SetOnlineState(false);

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
                    var msg = new ChatMessage(reader);
                    var from = ConnectionIdToUserId[peer.ConnectId];
                    string to = msg.FromOrTo;
                    msg.FromOrTo = from;

                    switch (msg.Scope)
                    {
                        case ChatScope.Whisper:
                            var aMsg = new ActorChatMessage()
                            {
                                Message = msg.Message,
                                Prefix = from,
                                Scope = msg.Scope
                            };

                            ActorProxy.Create<IChatActor>(new ActorId(from)).WriteMessage(aMsg);
                            ActorProxy.Create<IChatActor>(new ActorId(to)).WriteMessage(aMsg);
                            break;
                        case ChatScope.Map:
                            ActorProxy.Create<IMapActorService>(new ActorId(Map[from])).SendChatMessage(msg).Wait();
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
                        var actor = ActorProxy.Create<IChatActor>(new ActorId(id));
                        actor.SetOnlineState(true);

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
