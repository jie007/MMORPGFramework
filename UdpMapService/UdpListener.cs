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
using CommonServer.UdpServiceHelper;
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

        public ConcurrentDictionary<string, MapPartitionRegistration> PlayerRegistrations = new ConcurrentDictionary<string, MapPartitionRegistration>();
        public UdpConnectionManagment UdpConnectionManagment = new UdpConnectionManagment();


        public void Update()
        {
            UdpManager.PollEvents();
            var peers = UdpManager.GetPeers();

            Dictionary<string, MapPartitionRegistration> mapPartitionRegistrations = new Dictionary<string, MapPartitionRegistration>();

            foreach (var registration in PlayerRegistrations)
            {
                string map = registration.Value.Map;

                if (!mapPartitionRegistrations.ContainsKey(map))
                {
                    mapPartitionRegistrations.Add(map, new MapPartitionRegistration(map));
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

                var user = UdpConnectionManagment.GetUser(closurePeer.ConnectId);
                if(user == null)
                    continue;

                string name = user.Name;
                var registration = PlayerRegistrations[name];

                UdpDataWriter writer = new UdpDataWriter();
                foreach (var position in mapData[user.Map])
                {
                    var pPartition = new MapPartition(position);

                    if (registration.Partitions.Contains(pPartition))
                    {
                        position.Write(writer);
                    }
                }

                closurePeer.Send(writer, ChannelType.UnreliableOrdered);
            }
        }

        public void OnPeerConnected(UdpPeer peer)
        {
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            var user = UdpConnectionManagment.RemoveUser(peer.ConnectId);
            if (user == null)
                return;

            ActorProxy.Create<IMapActorService>(new ActorId(user.Map)).RemovePlayer(user.Name);
            MapPartitionRegistration removedPartitions = new MapPartitionRegistration(user.Map);
            PlayerRegistrations.TryRemove(user.Name, out removedPartitions);
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            try
            {
                UdpUser user = UdpConnectionManagment.GetUser(peer.ConnectId);
                if (user != null)
                {
                    var msg = new PositionMessage(reader);
                    msg.Name = user.Name;
                    PlayerRegistrations[msg.Name].Update(msg);
                    ActorProxy.Create<IMapActorService>(new ActorId(user.Map)).UpdatePlayerPosition(msg);
                }
                else
                {
                    var tokenMsg = new TokenMessage(reader);

                    user = UdpConnectionManagment.AddUser(peer.ConnectId, tokenMsg);
                    if (user != null)
                    {
                        PlayerRegistrations[user.Name] = new MapPartitionRegistration(user.Map);

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
