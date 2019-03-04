using System;
using ReliableUdp.Utility;

namespace Common.Protocols.Map
{
    public class PositionMessage
    {
        public string Name { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Speed { get; set; }

        public float Rotation { get; set; }

        public int CurrentAnimation { get; set; }

        public PositionMessage()
        {

        }

        public PositionMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)MessageTypes.Position)
            {
                throw new NotSupportedException();
            }

            reader.GetByte();

            Name = reader.GetString();
            X = reader.GetFloat();
            Y = reader.GetFloat();
            Z = reader.GetFloat();
            Rotation = reader.GetFloat();
            Speed = reader.GetFloat();
            CurrentAnimation = reader.GetInt();
        }

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)MessageTypes.Position);
            writer.Put(Name);
            writer.Put(X);
            writer.Put(Y);
            writer.Put(Z);
            writer.Put(Rotation);
            writer.Put(Speed);
            writer.Put(CurrentAnimation);
        }
    }
}