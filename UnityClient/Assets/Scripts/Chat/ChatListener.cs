using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Protocols.Chat;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using UnityEngine;

namespace Assets.Scripts.Chat
{
    public class ChatListener : IUdpEventListener
    {
        private readonly Action<bool> connEvent;
        private readonly Action tokenApproved;
        private readonly Action<ChatMessage> recvChatMessage;

        public ChatListener(Action<bool> connEvent, Action tokenApproved, Action<ChatMessage> recvChatMessage)
        {
            this.connEvent = connEvent;
            this.tokenApproved = tokenApproved;
            this.recvChatMessage = recvChatMessage;
        }

        public UdpManager UdpManager { get; set; }

        public void OnPeerConnected(UdpPeer peer)
        {
            Debug.Log("Connected. (" + UdpManager.GetFirstPeer().ConnectId + ")");
            connEvent(true);
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("Disconnected");
            connEvent(false);
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
            Debug.Log("[Client] error: " + socketErrorCode);
            connEvent(false);
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            if (reader.PeekByte() == (byte)ChatUdpProtocolMessageTypes.Token)
            {
                tokenApproved();
                Debug.Log("Token got approved!");
                return;
            }

            while (!reader.EndOfData)
            {
                var msg = new ChatMessage(reader);
                recvChatMessage(msg);
                Debug.Log(string.Format("{0} ({1}): {2}", msg.FromOrTo, msg.Scope, msg.Message));
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
