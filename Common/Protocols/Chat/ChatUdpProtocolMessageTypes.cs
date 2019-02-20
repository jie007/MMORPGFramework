using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Protocols.Chat
{
    public enum ChatUdpProtocolMessageTypes : byte
    {
        Token = 0,
        Chat = 1
    }
}
