using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace CommonServer
{
    public static class JwtTokenHelper
    {
        // TODO: Add Certificate to JWT Token
        public static readonly string SymmetricKey = "HelloWorld!HelloWorld!HelloWorld!HelloWorld!";

        public static string GenerateToken(string id, int expireHours = 48)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, id)
                }),
                Expires = now.AddHours(expireHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(UTF7Encoding.UTF7.GetBytes(SymmetricKey)), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return tokenHandler.WriteToken(stoken);
        }

        public static string GetTokenId(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var stoken = tokenHandler.ReadJwtToken(token);

                var now = DateTime.UtcNow;
                if (now >= stoken.ValidFrom && now <= stoken.ValidTo)
                {
                    return stoken.Claims.First(x => x.Type == "unique_name").Value;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
