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
using Common.Protocols.Map;
using CommonServer;
using CommonServer.ServiceFabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace UdpMapService
{
    public class UdpListener : IUdpListener
    {
        public UdpManager UdpManager { get; set; }

        public ConcurrentDictionary<long, string> ConnectionIdToUserId = new ConcurrentDictionary<long, string>();
        public ConcurrentDictionary<string, PositionMessage> LastPositions = new ConcurrentDictionary<string, PositionMessage>();

        public void Update()
        {
            UdpManager.PollEvents();
            var peers = UdpManager.GetPeers();

            UdpDataWriter writer = new UdpDataWriter();
            foreach (var position in LastPositions)
            {
                var positionMsg = new PositionMessage()
                {
                    Name = position.Key,
                    X = position.Value.X,
                    Z = position.Value.Z,
                    Rotation = position.Value.Rotation,
                    Speed = position.Value.Speed
                };
                positionMsg.Write(writer);
            }

            foreach (UdpPeer peer in peers)
            {
                var closurePeer = peer;
                if (!this.ConnectionIdToUserId.ContainsKey(closurePeer.ConnectId))
                    continue;

                closurePeer.Send(writer, ChannelType.ReliableOrdered);
            }
        }

        public void OnPeerConnected(UdpPeer peer)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            if (ConnectionIdToUserId.ContainsKey(peer.ConnectId))
            {
                string value = string.Empty;
                ConnectionIdToUserId.TryRemove(peer.ConnectId, out value);
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
                    var msg = new PositionMessage(reader);

                    string name = ConnectionIdToUserId[peer.ConnectId];
                    msg.Name = name;

                    LastPositions.AddOrUpdate(name, msg, (key, oldValue) => msg);
                }
                else
                {
                    var tokenMsg = new TokenMessage(reader);
                    string token = tokenMsg.Token;
                    string id = JwtTokenHelper.GetTokenId(token, "CharacterName");

                    if (string.IsNullOrEmpty(id))
                    {
                        UdpManager.DisconnectPeer(peer);
                    }
                    else
                    {
                        ConnectionIdToUserId.AddOrUpdate(peer.ConnectId, (connId) => { return id; }, (connId, oldVal) => { return id; });
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
