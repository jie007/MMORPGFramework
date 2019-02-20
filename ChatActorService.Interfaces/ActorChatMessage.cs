using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Protocols.Chat;

namespace ChatActorService.Interfaces
{
    [Serializable]
    public class ActorChatMessage
    {
        public string Prefix { get; set; }

        public string Message { get; set; }

        public ChatScope Scope { get; set; }
    }
}
