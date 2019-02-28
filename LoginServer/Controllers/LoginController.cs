using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.RestApi;
using CommonServer;
using CommonServer.DocDb;
using CommonServer.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace LoginServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController : ControllerBase
    {
        private UserInformationRepository repo = new UserInformationRepository();
        private static Regex emailRegex = new Regex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$");

        [Route("token")]
        [HttpPost]
        public async Task<ActionResult<string>> GetToken([FromBody] LoginInformation loginInformation)
        {
            try
            {
                var document = await repo.Get(loginInformation.Email);

                if (!PasswordHash.CheckPassword(document.PasswordHash, loginInformation.Password))
                {
                    return string.Empty;
                }
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    return string.Empty;
                }
            }

            return JwtTokenHelper.GenerateToken(ClaimTypes.Email, loginInformation.Email);
        }

        [Route("register")]
        [HttpPost]
        public async Task<ActionResult<RegisterResult>> Register([FromBody] LoginInformation loginInformation)
        {
            if (string.IsNullOrEmpty(loginInformation.Email))
            {
                return RegisterResult.NoEmail;
            }

            if (string.IsNullOrEmpty(loginInformation.Password))
            {
                return RegisterResult.NoPassword;
            }

            if (loginInformation.Password.Length < 8)
            {
                return RegisterResult.PasswordNotStrongEnough;
            }

            if (!emailRegex.IsMatch(loginInformation.Email))
            {
                return RegisterResult.IncorrectEmail;
            }

            var userInfo = new UserInformation()
            {
                Id = loginInformation.Email,
                PasswordHash = PasswordHash.GetPasswordHash(loginInformation.Password)
            };

            if (!await repo.Create(userInfo))
            {
                return RegisterResult.AlreadyRegistered;
            }

            return RegisterResult.Ok;
        }
    }
}
