using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using Newtonsoft.Json;

namespace Assets.Api
{
    public static class RestApi
    {
        private const string AuthorizationHeaderKey = "Authorization";
        private const string AuthorizationType = "Bearer";

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApi"/> class.
        /// </summary>
        static RestApi()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
            {
                return true;
            };

        }

        private static readonly string basePath = "https://localhost:8557";
        private static LoginApi loginApi = new LoginApi(basePath);
        private static CharacterApi characterApi = new CharacterApi(basePath);

        public static string CharacterToken { get; private set; }
        public static string CharacterName { get; private set; }

        public static string Login(string email, string password)
        {
            string token = JsonConvert.DeserializeObject<string>(loginApi.GetToken(new LoginInformation(email, password)));
            characterApi.Configuration.AddApiKeyPrefix(AuthorizationHeaderKey, AuthorizationType);
            characterApi.Configuration.AddApiKey(AuthorizationHeaderKey, token);
            return token;
        }

        public static Common.RestApi.RegisterResult Register(string email, string password)
        {
            return (Common.RestApi.RegisterResult)loginApi.Register(new LoginInformation(email, password)).Value;
        }

        public static List<CharacterInformation> GetCharacters()
        {
            return characterApi.GetCharacters();
        }

        public static CharacterInformation CreateCharacter(string name)
        {
            return characterApi.CreateCharacter(name);
        }

        public static string SelectCharacter(string name)
        {
            CharacterName = name;
            CharacterToken = JsonConvert.DeserializeObject<string>(characterApi.SelectCharacter(name));
            return CharacterToken;
        }
    }
}
