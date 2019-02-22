using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Api;
using Common.Protocols.Chat;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Chat
{
    public class ChatClient : MonoBehaviour
    {
        public ChatWindow Chat;

        private UdpManager manager;
        private bool connected = false;

        public void Awake()
        {
            FactoryRegistrations.Register();

            string token = RestApi.CharacterToken;

            bool connecting = true;
            manager = new UdpManager(new ChatListener((con) =>
            {
                connected = con;
                Debug.Log("Chat Client Connected");
                connecting = false;
            }, () =>
            {
                Chat.SendChatMessage = SendChatMessage;
            }, RecievedChatMessage), "kds");

            manager.Connect("localhost", 9191);
            Debug.Log("Chat Client Connecting...");

            SendToken(token);
        }

        private void SendChatMessage(ChatMessage msg)
        {
            var chatWriter = new UdpDataWriter();
            msg.Write(chatWriter);
            manager.SendToAll(chatWriter, ChannelType.ReliableOrdered);
        }

        private void RecievedChatMessage(ChatMessage msg)
        {
            Chat.AddMessage(msg);
        }

        public void Update()
        {
            if (manager != null)
            {
                manager.PollEvents();
            }
        }
        private void SendToken(string token)
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
