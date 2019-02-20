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
    class Program
    {
        public static void Main(string[] args)
        {
            FactoryRegistrations.Register();
            Console.WriteLine("Please enter valid Token:");
            string token = Console.ReadLine();

            bool connected = false;
            bool connecting = true;
            var manager = new UdpManager(new ClientListener((con) =>
            {
                connected = con;
                connecting = false;
            }), "kds");

            manager.Connect("localhost", 9191);
            Console.WriteLine("Connecting...");

            while (connecting)
            {
                manager.PollEvents();
                System.Threading.Thread.Sleep(100);
            }

            var thread = new UdpThread("Pool Events", 100,
                () =>
                {
                    manager.PollEvents();
                });
            thread.Start();

            SendToken(token, manager);

            while (connected)
            {
                var msg = new ChatMessage();
                Console.WriteLine("Who to send a Message (empty for Map)?");
                msg.FromOrTo = Console.ReadLine();
                
                msg.Scope = ChatScope.Whisper;
                if (string.IsNullOrEmpty(msg.FromOrTo))
                {
                    msg.Scope = ChatScope.Map;
                }

                Console.WriteLine("Message?");
                msg.Message = Console.ReadLine();

                var writer = new UdpDataWriter();
                msg.Write(writer);
                manager.SendToAll(writer, ChannelType.ReliableOrdered);
            }

            if (thread != null)
                thread.Stop();

            Console.WriteLine("Stopped Execution.");

        }

        private static void SendToken(string token, UdpManager manager)
        {
            var tokenMsg = new TokenMessage()
            {
                Token = token
            };
            var tokenWriter = new UdpDataWriter();
            tokenMsg.Write(tokenWriter);
            manager.SendToAll(tokenWriter, ChannelType.ReliableOrdered);
        }
    }
}
