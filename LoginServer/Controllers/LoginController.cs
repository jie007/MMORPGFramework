using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.RestApi;
using CommonServer;
using CommonServer.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace LoginServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private static Regex emailRegex = new Regex("^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*$");

        [Route("checkToken")]
        [HttpPost]
        public ActionResult<string> CheckToken(string token)
        {
            return JwtTokenHelper.GetTokenId(token);
        }

        [Route("token")]
        [HttpPost]
        public async Task<ActionResult<string>> GetToken([FromBody] LoginInformation loginInformation)
        {
            var docDb = await GetDocumentDbClient();

            try
            {
                var document = await docDb.ReadDocumentAsync<UserInformation>(UriFactory.CreateDocumentUri(DocumentDbConfiguration.DocumentDbLoginDbId, DocumentDbConfiguration.DocumentDbLoginDbCollection, loginInformation.Email));

                if (!PasswordHash.CheckPassword(document.Document.PasswordHash, loginInformation.Password))
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

            return JwtTokenHelper.GenerateToken(loginInformation.Email);
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

            var docDb = await GetDocumentDbClient();

            try
            {
                var userInfo = new UserInformation()
                {
                    Id = loginInformation.Email,
                    PasswordHash = PasswordHash.GetPasswordHash(loginInformation.Password)
                };

                await docDb.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DocumentDbConfiguration.DocumentDbLoginDbId, DocumentDbConfiguration.DocumentDbLoginDbCollection), userInfo);
            }
            catch (DocumentClientException de)
            {
                return RegisterResult.AlreadyRegistered;
            }

            return RegisterResult.Ok;
        }

        private static async Task<DocumentClient> GetDocumentDbClient()
        {
            var secureString = new SecureString();

            foreach (var c in DocumentDbConfiguration.DocumentDbKey)
            {
                secureString.AppendChar(c);
            }

            var docDb = new DocumentClient(new Uri(DocumentDbConfiguration.DocumentDbEndpointUrl), secureString);
            await docDb.CreateDatabaseIfNotExistsAsync(new Database() {Id = DocumentDbConfiguration.DocumentDbLoginDbId});
            await docDb.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DocumentDbConfiguration.DocumentDbLoginDbId),
                new DocumentCollection {Id = DocumentDbConfiguration.DocumentDbLoginDbCollection});

            return docDb;
        }
    }
}
