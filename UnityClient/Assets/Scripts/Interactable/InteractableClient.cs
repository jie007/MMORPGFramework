using System.Linq;
using Assets.Api;
using Common.Protocols;
using Common.Protocols.Interactable;
using Common.Protocols.Map;
using ReliableUdp;
using ReliableUdp.Enums;
using ReliableUdp.Utility;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class InteractableClient : MonoBehaviour
    {
        private UdpManager manager;
        private bool connected = false;

        public void Awake()
        {
            FactoryRegistrations.Register();

            string token = RestApi.CharacterToken;

            bool connecting = true;
            manager = new UdpManager(new InteractableListener((con) =>
            {
                SendToken(token);
                connected = con;
                Debug.Log("Interactable Client Connected");
                connecting = false;
            }, () =>
            {
            }, ReceivedStatus), "kds");

            manager.Connect("localhost", 9193);
            Debug.Log("Interactable Client Connecting...");
        }

        public void SendStartMessage(string interactableId)
        {
            var writer = new UdpDataWriter();
            var msg = new StartInteractMessage(interactableId);
            msg.Write(writer);
            manager.SendToAll(writer, ChannelType.ReliableOrdered);
        }

        public void SendFinishMessage(string interactableId)
        {
            var writer = new UdpDataWriter();
            var msg = new FinishInteractMessage(interactableId);
            msg.Write(writer);
            manager.SendToAll(writer, ChannelType.ReliableOrdered);
        }

        private void ReceivedStatus(InteractableStatusMessage msg)
        {
            Debug.Log("Interaction status is " + msg.IsActive + " for " + msg.Id);
            var interactables = GameObject.FindObjectsOfType<InteractableBehaviour>();
            var interactable = interactables.FirstOrDefault(x => x.Konfiguration.Id == msg.Id);

            if (interactable == null)
                return;

            interactable.SetInteractableState(msg.IsActive);
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
