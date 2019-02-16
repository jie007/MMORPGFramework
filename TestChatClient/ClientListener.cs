using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Console.WriteLine(reader.GetString());
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
