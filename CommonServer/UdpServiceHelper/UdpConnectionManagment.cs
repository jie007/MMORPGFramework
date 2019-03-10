using System;
using System.Collections.Concurrent;
using Common.Protocols;

namespace CommonServer.UdpServiceHelper
{
    public class UdpConnectionManagment
    {
        public ConcurrentDictionary<long, UdpUser> ConnectionIdToUserId = new ConcurrentDictionary<long, UdpUser>();

        public UdpUser GetUser(long id)
        {
            if (!ConnectionIdToUserId.ContainsKey(id))
                return null;

            return ConnectionIdToUserId[id];
        }

        public UdpUser AddUser(long id, TokenMessage tokenMsg)
        {
            try
            {
                string token = tokenMsg.Token;
                string name = JwtTokenHelper.GetTokenClaim(token, "CharacterName");
                string map = JwtTokenHelper.GetTokenClaim(token, "Map");

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(map))
                    return null;

                var result = new UdpUser(name, map);
                ConnectionIdToUserId[id] = result;

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public UdpUser RemoveUser(long id)
        {
            UdpUser user;
            ConnectionIdToUserId.TryRemove(id, out user);

            return user;
        }
    }
}
