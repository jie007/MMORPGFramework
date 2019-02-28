using System;
using ReliableUdp.Utility;

namespace Common.Protocols
{
    public class TokenMessage
    {
        public string Token { get; set; }

        public TokenMessage()
        {

        }

        public TokenMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)MessageTypes.Token)
            {
                throw new NotSupportedException();
            }

            reader.GetByte();

            Token = reader.GetString();
        }

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)MessageTypes.Token);
            writer.Put(Token);
        }
    }
}