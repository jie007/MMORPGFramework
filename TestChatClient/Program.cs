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

            var tokenWriter = new UdpDataWriter();
            tokenWriter.Put(token);
            manager.SendToAll(tokenWriter, ChannelType.ReliableOrdered);

            while (connected)
            {
                string message = Console.ReadLine();
                var writer = new UdpDataWriter();
                writer.Put(message);
                manager.SendToAll(writer, ChannelType.ReliableOrdered);
            }

            if (thread != null)
                thread.Stop();

            Console.WriteLine("Stopped Execution.");

        }
    }
}
