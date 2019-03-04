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
using CommonServer.MapPartitioning;
using CommonServer.ServiceFabric;
using MapActorService.Interfaces;
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
        public ConcurrentDictionary<string, string> Map = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, MapPartitionRegistration> PlayerRegistrations = new ConcurrentDictionary<string, MapPartitionRegistration>();

        public void Update()
        {
            UdpManager.PollEvents();
            var peers = UdpManager.GetPeers();

            Dictionary<string, MapPartitionRegistration> mapPartitionRegistrations = new Dictionary<string, MapPartitionRegistration>();

            foreach (var registration in PlayerRegistrations)
            {
                string map = Map[registration.Key];

                if (!mapPartitionRegistrations.ContainsKey(map))
                {
                    mapPartitionRegistrations.Add(map, new MapPartitionRegistration());
                }

                mapPartitionRegistrations[map].Merge(registration.Value);
            }

            Dictionary<string, List<PositionMessage>> mapData = new Dictionary<string, List<PositionMessage>>();

            foreach (var mapRequest in mapPartitionRegistrations)
            {
                mapData.Add(mapRequest.Key, ActorProxy.Create<IMapActorService>(new ActorId(mapRequest.Key)).GetPlayer(mapRequest.Value.Partitions).Result);
            }

            foreach (UdpPeer peer in peers)
            {
                var closurePeer = peer;
                if (!this.ConnectionIdToUserId.ContainsKey(closurePeer.ConnectId))
                    continue;

                string name = ConnectionIdToUserId[closurePeer.ConnectId];
                var registration = PlayerRegistrations[name];

                UdpDataWriter writer = new UdpDataWriter();
                foreach (var position in mapData[Map[name]])
                {
                    var pPartition = new MapPartition(position);

                    if (registration.Partitions.Contains(pPartition))
                    {
                        var positionMsg = new PositionMessage()
                        {
                            Name = position.Name,
                            X = position.X,
                            Y = position.Y,
                            Z = position.Z,
                            Rotation = position.Rotation,
                            Speed = position.Speed
                        };
                        positionMsg.Write(writer);
                    }
                }

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
                string name = ConnectionIdToUserId[peer.ConnectId];
                ConnectionIdToUserId.TryRemove(peer.ConnectId, out value);

                string map = string.Empty;
                Map.TryRemove(name, out map);

                if (!string.IsNullOrEmpty(map))
                {
                    ActorProxy.Create<IMapActorService>(new ActorId(map)).RemovePlayer(name);
                }

                MapPartitionRegistration removedPartitions = new MapPartitionRegistration();
                PlayerRegistrations.TryRemove(name, out removedPartitions);
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

                    PlayerRegistrations[msg.Name].Update(msg);

                    string map = Map[msg.Name];
                    ActorProxy.Create<IMapActorService>(new ActorId(map)).UpdatePlayerPosition(msg);
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
                        PlayerRegistrations.AddOrUpdate(id, (connId) => { return new MapPartitionRegistration(); }, (connId, oldVal) => { return new MapPartitionRegistration(); });

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
