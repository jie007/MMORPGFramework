using System;
using ReliableUdp.Utility;

namespace Common.Protocols.Map
{
    public class FinishInteractMessage
    {
        public string Id { get; set; }

        public long ClientTimeStamp { get; set; }

        public long ServerTimeStamp { get; set; }

        public FinishInteractMessage()
        {

        }

        public FinishInteractMessage(string id)
        {
            Id = id;
            ClientTimeStamp = DateTime.UtcNow.Ticks;
            ServerTimeStamp = DateTime.UtcNow.Ticks;
        }

        public FinishInteractMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)MessageTypes.InteractableFinish)
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
            writer.Put((byte)MessageTypes.InteractableFinish);
            writer.Put(Id);
            writer.Put(ClientTimeStamp);
            writer.Put(ServerTimeStamp);
        }
    }
}