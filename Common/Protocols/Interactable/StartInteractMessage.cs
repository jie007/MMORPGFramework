using System;
using ReliableUdp.Utility;

namespace Common.Protocols.Map
{
    public class StartInteractMessage
    {
        public string Id { get; set; }

        public long ClientTimeStamp { get; set; }

        public long ServerTimeStamp { get; set; }

        public StartInteractMessage()
        {

        }

        public StartInteractMessage(string id)
        {
            Id = id;
            ClientTimeStamp = DateTime.UtcNow.Ticks;
            ServerTimeStamp = DateTime.UtcNow.Ticks;
        }

        public StartInteractMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)MessageTypes.InteractableStart)
            {
                throw new NotSupportedException();
            }

            reader.GetByte();

            Id = reader.GetString();
            ClientTimeStamp = reader.GetLong();
            ServerTimeStamp = reader.GetLong();
        }

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)MessageTypes.InteractableStart);
            writer.Put(Id);
            writer.Put(ClientTimeStamp);
            writer.Put(ServerTimeStamp);
        }
    }
}