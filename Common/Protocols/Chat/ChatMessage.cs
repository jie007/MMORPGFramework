using System;
using ReliableUdp.Utility;

namespace Common.Protocols.Chat
{
    public class ChatMessage
    {
        public ChatScope Scope { get; set; }

        public string FromOrTo { get; set; }

        public string Message { get; set; }

        public ChatMessage()
        {

        }

        public ChatMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)ChatUdpProtocolMessageTypes.Chat)
            {
                throw new NotSupportedException();
            }

            reader.GetByte();

            Scope = (ChatScope) reader.GetByte();
            FromOrTo = reader.GetString();
            Message = reader.GetString();
        }

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)ChatUdpProtocolMessageTypes.Chat);
            writer.Put((byte)Scope);
            writer.Put(FromOrTo);
            writer.Put(Message);
        }
    }
}