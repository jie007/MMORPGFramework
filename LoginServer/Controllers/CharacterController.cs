using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Common.RestApi;
using CommonServer;
using CommonServer.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace LoginServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CharacterController : ControllerBase
    {
        [Route("SelectCharacter")]
        [HttpGet]
        public async Task<ActionResult<string>> SelectCharacter(string name)
        {
            var email = GetEmailClaim();

            var c = await DocumentClientSinglton.Instance.ReadDocumentAsync<CharacterInformation>(UriFactory.CreateDocumentUri(DocumentDbConfiguration.DocumentDb, DocumentDbConfiguration.DocumentDbCharacterDbCollection, name));

            if (c.Document.OwnerEmail != email)
            {
                return string.Empty;
            }

            return JwtTokenHelper.GenerateToken("CharacterName", name);
        }

        [Route("GetCharacters")]
        [HttpGet]
        public ActionResult<List<CharacterInformation>> GetCharacters()
        {
            return GetCharacters(GetEmailClaim());
        }

        private string GetEmailClaim()
        {
            return HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Email).Value;
        }


        [Route("CreateCharacter")]
        [HttpPost]
        public async Task<ActionResult<CharacterInformation>> CreateCharacter(string name)
        {
            string email = GetEmailClaim();

            if (await ExistsCharacter(name))
                return null;

            if (GetCharacters(email).Count > GameDesign.MaximumNumberOfCharacters)
                return null;

            var newChar = new CharacterInformation()
            {
                Id = name,
                OwnerEmail = email,
                Map = "StartMap"
            };

            await DocumentClientSinglton.Instance.CreateDocumentAsync(DocumentDbConfiguration.CharacterCollectionUri, newChar);

            return newChar;
        }

        private static List<CharacterInformation> GetCharacters(string email)
        {
            var docDb = DocumentClientSinglton.Instance;
            var q = from c in docDb.CreateDocumentQuery<CharacterInformation>(DocumentDbConfiguration.CharacterCollectionUri)
                    where c.OwnerEmail == email
                    select c;

            return q.ToList();
        }

        private static async Task<bool> ExistsCharacter(string name)
        {
            var docDb = DocumentClientSinglton.Instance;
            try
            {
                var document = await docDb.ReadDocumentAsync<CharacterInformation>(
                    UriFactory.CreateDocumentUri(DocumentDbConfiguration.DocumentDb,
                        DocumentDbConfiguration.DocumentDbCharacterDbCollection, name));

                return true;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode != HttpStatusCode.NotFound)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
