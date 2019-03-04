using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocols
{
    public enum MessageTypes : byte
    {
        Token = 0,
        Chat = 1,
        Position = 2,
        InteractableStart = 3,
        InteractableFinish = 4,
        InteractableStatus = 5
    }
}
