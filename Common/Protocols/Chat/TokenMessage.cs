using System;
using ReliableUdp.Utility;

namespace Common.Protocols.Chat
{
    public class TokenMessage
    {
        public string Token { get; set; }

        public TokenMessage()
        {

        }

        public TokenMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)ChatUdpProtocolMessageTypes.Token)
            {
                throw new NotSupportedException();
            }

            reader.GetByte();

            Token = reader.GetString();
        }

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)ChatUdpProtocolMessageTypes.Token);
            writer.Put(Token);
        }
    }
}