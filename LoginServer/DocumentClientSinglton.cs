using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using CommonServer;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace LoginServer
{
    public static class DocumentClientSinglton
    {
        private static DocumentClient instance;

        private static object lockObject = new object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DocumentClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = GetDocumentDbClient().Result;
                        }
                    }
                }

                return instance;
            }
        }

        private static async Task<DocumentClient> GetDocumentDbClient()
        {
            var secureString = new SecureString();

            foreach (var c in DocumentDbConfiguration.DocumentDbKey)
            {
                secureString.AppendChar(c);
            }

            var docDb = new DocumentClient(new Uri(DocumentDbConfiguration.DocumentDbEndpointUrl), secureString);
            await docDb.CreateDatabaseIfNotExistsAsync(new Database() { Id = DocumentDbConfiguration.DocumentDb });
            await docDb.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DocumentDbConfiguration.DocumentDb),
                new DocumentCollection { Id = DocumentDbConfiguration.DocumentDbLoginDbCollection });
            await docDb.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DocumentDbConfiguration.DocumentDb),
                new DocumentCollection { Id = DocumentDbConfiguration.DocumentDbCharacterDbCollection });

            return docDb;
        }

    }
}
