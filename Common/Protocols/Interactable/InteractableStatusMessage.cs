using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliableUdp.Utility;

namespace Common.Protocols.Interactable
{
    [Serializable]
    public class InteractableStatusMessage : ICloneable
    {
        public string Id;

        public bool IsActive;

        public InteractableStatusMessage()
        {

        }

        public InteractableStatusMessage(string id, bool isActive)
        {
            Id = id;
            IsActive = isActive;
        }

        public InteractableStatusMessage(UdpDataReader reader)
        {
            if (reader.PeekByte() != (byte)MessageTypes.InteractableStatus)
            {
                throw new NotSupportedException();
            }

            reader.GetByte();

            Id = reader.GetString();
            IsActive = reader.GetBool();
        }

        public void Write(UdpDataWriter writer)
        {
            writer.Put((byte)MessageTypes.InteractableStatus);
            writer.Put(Id);
            writer.Put(IsActive);
        }

        public object Clone()
        {
            return new InteractableStatusMessage(this.Id, this.IsActive);
        }
    }
}
