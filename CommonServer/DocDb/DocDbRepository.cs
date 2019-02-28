using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonServer.Documents;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CommonServer.DocDb
{
    public abstract class DocDbRepository<T> where T : class
    {
        protected string DocDbUri;
        protected string CollectoinName;
        protected Uri CollectionUri;

        protected DocDbRepository(string docDbUri, string collectionName)
        {
            this.DocDbUri = docDbUri;
            this.CollectoinName = collectionName;
            CollectionUri = UriFactory.CreateDocumentCollectionUri(DocDbUri, CollectoinName);
        }

        public async Task<T> Get(string key)
        {
            var docDb = DocumentClientSinglton.Instance;

            try
            {
                return (await docDb.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(DocDbUri, CollectoinName, key))).Document;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
            }

            return null;
        }

        public async Task<bool> Exists(string key)
        {
            return await Get(key) != null;
        }

        public async Task<bool> Create(T obj)
        {
            try
            {
                await DocumentClientSinglton.Instance.CreateDocumentAsync(CollectionUri, obj);
            }
            catch (DocumentClientException e)
            {
                return false;
            }

            return true;
        }

        public async Task Save(T obj)
        {
            await DocumentClientSinglton.Instance.UpsertDocumentAsync(CollectionUri, obj);
        }
    }
}
