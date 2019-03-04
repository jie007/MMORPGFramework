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
        public PlayerController PlayerController;
        public GameObject OtherPlayerPrefab;
        private UdpManager manager;
        private bool connected = false;

        private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
        private Dictionary<string, PositionMessage> playerPositions = new Dictionary<string, PositionMessage>();

        public float LerpSpeed = 0.1f;

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
            Debug.Log("XYZ: " + msg.X + "," + msg.Y + "," + msg.Z);
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
                playerPositions.Add(msg.Name, msg);
                players.Add(msg.Name, GameObject.Instantiate(OtherPlayerPrefab));
            }

            playerPositions[msg.Name] = msg;
        }

        public void Update()
        {
            if (manager != null)
            {
                manager.PollEvents();
            }

            var playerNames = players.Keys.ToList();
            foreach (var name in playerNames)
            {
                var msg = playerPositions[name];
                players[msg.Name].transform.rotation = Quaternion.Euler(0, msg.Rotation, 0);
                players[msg.Name].transform.position = Vector3.Lerp(players[msg.Name].transform.position, new Vector3(msg.X, msg.Y, msg.Z), LerpSpeed);
                players[msg.Name].GetComponent<RemotePlayerAnimation>().CurrentSpeed = Mathf.Lerp(players[msg.Name].GetComponent<RemotePlayerAnimation>().CurrentSpeed, msg.Speed, LerpSpeed);
            }
        }

        public void FixedUpdate()
        {
            if (connected)
            {
                SendPosition(new PositionMessage()
                {
                    Name = string.Empty,
                    X = PlayerController.transform.position.x,
                    Y = PlayerController.transform.position.y,
                    Z = PlayerController.transform.position.z,
                    Speed = PlayerController.AnimationsSpeed,
                    Rotation = PlayerController.transform.rotation.eulerAngles.y
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
