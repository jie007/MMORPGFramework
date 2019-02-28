using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace CommonServer
{
    public static class DocumentDbConfiguration
    {
        public static readonly string DocumentDbEndpointUrl = "https://localhost:8081";
        public static readonly string DocumentDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        public static readonly string DocumentDb = "LoginDb";
        public static readonly string DocumentDbLoginDbCollection = "LoginCollection";
        public static readonly string DocumentDbCharacterDbCollection = "CharacterCollection";
    }
}
