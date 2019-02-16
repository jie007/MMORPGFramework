using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatActorService.Interfaces;
using CommonServer;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace UdpChatService
{
    public class UdpListener : IUdpEventListener
    {
        public UdpManager UdpManager { get; set; }

        public ConcurrentDictionary<long, string> ConnectionIdToUserId = new ConcurrentDictionary<long, string>();

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
                    if (taskMessages.Result.Count == 0)
                        return;

                    var joinedMessage = string.Join("\r\n", taskMessages.Result);
                    UdpDataWriter writer = new UdpDataWriter();
                    writer.Put(joinedMessage);
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
            }
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            if (ConnectionIdToUserId.ContainsKey(peer.ConnectId))
            {
                string message = reader.GetString();

                var chatInfo = message.Split(new char[] { ' ' }, 2);
                if (chatInfo.Length == 2)
                {
                    var from = ConnectionIdToUserId[peer.ConnectId];
                    var messageFrom = string.Format("{0}: {1}", from, chatInfo[1]);
                    var actor = ActorProxy.Create<IChatActor>(new ActorId(chatInfo[0]));
                    actor.WriteMessage(messageFrom).Wait();

                    UdpDataWriter writer = new UdpDataWriter();
                    writer.Put(messageFrom);
                    peer.Send(writer, ChannelType.ReliableOrdered);
                }
            }
            else
            {
                string token = reader.GetString();
                string id = JwtTokenHelper.GetTokenId(token);

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
                    writer.Put(token);
                    peer.Send(writer, ChannelType.ReliableOrdered);
                }
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
