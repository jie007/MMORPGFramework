using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Protocols;
using Common.Protocols.Chat;
using Common.Protocols.Interactable;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class InteractableListener : IUdpEventListener
    {
        private readonly Action<bool> connEvent;
        private readonly Action tokenApproved;
        private readonly Action<InteractableStatusMessage> recvMessage;

        public InteractableListener(Action<bool> connEvent, Action tokenApproved, Action<InteractableStatusMessage> recvMessage)
        {
            this.connEvent = connEvent;
            this.tokenApproved = tokenApproved;
            this.recvMessage = recvMessage;
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
            if (reader.PeekByte() == (byte)MessageTypes.Token)
            {
                tokenApproved();
                Debug.Log("Interactable Token got approved!");
                return;
            }

            while (!reader.EndOfData)
            {
                var msg = new InteractableStatusMessage(reader);
                recvMessage(msg);
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
