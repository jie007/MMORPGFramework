using System;
using Assets.Api;
using Common.Protocols.Chat;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class ChatWindow : MonoBehaviour
    {
        public GameObjectPool Pool;
        public Transform Parent;
        public TMP_InputField ChatInput;

        public Action<ChatMessage> SendChatMessage { get; set; }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string input = ChatInput.text;

                if (input.StartsWith("/"))
                {
                    string[] inputSplitted = input.Split(new[] {' '}, 3);

                    if (inputSplitted.Length == 3)
                    {
                        if (inputSplitted[0] == "/w")
                        {
                            ChatMessage msg = new ChatMessage()
                            {
                                FromOrTo = inputSplitted[1],
                                Message = inputSplitted[2],
                                Scope = ChatScope.Whisper
                            };
                            if (SendChatMessage != null)
                            {
                                SendChatMessage(msg);
                            }
                        }
                    }
                }
                else
                {
                    ChatMessage msg = new ChatMessage()
                    {
                        FromOrTo = RestApi.CharacterName,
                        Message = input,
                        Scope = ChatScope.Map
                    };
                    if (SendChatMessage != null)
                    {
                        SendChatMessage(msg);
                    }
                }


                ChatInput.text = string.Empty;
            }
        }

        public void AddMessage(ChatMessage msg)
        {
            var go = Pool.Get();
            go.transform.SetParent(Parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();

            Color textColor = Color.white;
            if (msg.FromOrTo == RestApi.CharacterName)
            {
                textColor = Color.gray;
            }
            else if (msg.Scope == ChatScope.System)
            {
                textColor = Color.red;
            }

            text.color = textColor;

            if (msg.Scope == ChatScope.Whisper || msg.Scope == ChatScope.Map)
            {
                text.text = string.Format("{0}: {1}", msg.FromOrTo, msg.Message);
            }
            else
            {
                text.text = string.Format("[System] {0}", msg.Message);
            }
        }
    }
}
