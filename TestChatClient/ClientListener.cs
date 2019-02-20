using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Protocols.Chat;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;

namespace TestChatClient
{
    public class ClientListener : IUdpEventListener
    {
        private readonly Action<bool> connEvent;


        public ClientListener(Action<bool> connEvent)
        {
            this.connEvent = connEvent;
        }

        public UdpManager UdpManager { get; set; }

        public void OnPeerConnected(UdpPeer peer)
        {
            Console.WriteLine("Connected. (" + UdpManager.GetFirstPeer().ConnectId + ")");
            connEvent(true);
        }

        public void OnPeerDisconnected(UdpPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("Disconnected");
            connEvent(false);
        }

        public void OnNetworkError(UdpEndPoint endPoint, int socketErrorCode)
        {
            Console.WriteLine("[Client] error: " + socketErrorCode);
            connEvent(false);
        }

        public void OnNetworkReceive(UdpPeer peer, UdpDataReader reader, ChannelType channel)
        {
            if (reader.PeekByte() == (byte) ChatUdpProtocolMessageTypes.Token)
            {
                Console.WriteLine("Token got approved!");
                return;
            }

            while (!reader.EndOfData)
            {
                var msg = new ChatMessage(reader);
                Console.WriteLine("{0} ({1}): {2}", msg.FromOrTo, msg.Scope, msg.Message);
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
