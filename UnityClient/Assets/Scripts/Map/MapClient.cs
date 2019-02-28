using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Api;
using Common.Protocols;
using Common.Protocols.Chat;
using Common.Protocols.Map;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Map
{
    public class MapClient : MonoBehaviour
    {
        public Transform PlayerPosition;
        public GameObject OtherPlayerPrefab;
        private UdpManager manager;
        private bool connected = false;

        private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

        public void Awake()
        {
            FactoryRegistrations.Register();

            string token = RestApi.CharacterToken;

            bool connecting = true;
            manager = new UdpManager(new MapListener((con) =>
            {
                SendToken(token);
                connected = con;
                Debug.Log("Chat Client Connected");
                connecting = false;
            }, () =>
            {
            }, ReceivedPositionMessage), "kds");

            manager.Connect("localhost", 9192);
            Debug.Log("Chat Client Connecting...");
        }

        private void SendPosition(PositionMessage msg)
        {
            var msgWriter = new UdpDataWriter();
            msg.Write(msgWriter);
            manager.SendToAll(msgWriter, ChannelType.UnreliableOrdered);
        }

        private void ReceivedPositionMessage(PositionMessage msg)
        {
            if (msg.Name == Context.Charname)
                return;

            if (!players.ContainsKey(msg.Name))
            {
                players.Add(msg.Name, GameObject.Instantiate(OtherPlayerPrefab));
            }

            players[msg.Name].transform.position = new Vector3(msg.X, 0, msg.Z);
        }

        public void Update()
        {
            if (manager != null)
            {
                manager.PollEvents();
            }
        }

        public void FixedUpdate()
        {
            if (connected)
            {
                SendPosition(new PositionMessage()
                {
                    Name = string.Empty,
                    X = PlayerPosition.transform.position.x,
                    Z = PlayerPosition.transform.position.z
                });
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
