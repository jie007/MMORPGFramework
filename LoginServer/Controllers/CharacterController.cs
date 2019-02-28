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
using CommonServer.DocDb;
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
        private CharacterInformationRepository repo = new CharacterInformationRepository();

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

            return JwtTokenHelper.GenerateToken(new List<Claim>
                {
                    new Claim("CharacterName", name),
                    new Claim("Map", c.Document.Map)
                });
        }

        [Route("GetCharacters")]
        [HttpGet]
        public ActionResult<List<CharacterInformation>> GetCharacters()
        {
            return repo.GetCharacters(GetEmailClaim());
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

            if (await repo.Exists(name))
                return null;

            if (repo.GetCharacters(email).Count > GameDesign.MaximumNumberOfCharacters)
                return null;

            var newChar = new CharacterInformation()
            {
                Id = name,
                OwnerEmail = email,
                Map = "StartMap"
            };

            if (!await repo.Create(newChar))
            {
                return null;
            }

            return newChar;
        }
    }
}
