using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServer.Documents;

namespace CommonServer.DocDb
{
    public class CharacterInformationRepository : DocDbRepository<CharacterInformation>
    {
        public CharacterInformationRepository() : base(DocumentDbConfiguration.DocumentDb, DocumentDbConfiguration.DocumentDbCharacterDbCollection)
        {

        }
        public List<CharacterInformation> GetCharacters(string email)
        {
            var docDb = DocumentClientSinglton.Instance;
            var q = from c in docDb.CreateDocumentQuery<CharacterInformation>(CollectionUri)
                where c.OwnerEmail == email
                select c;

            return q.ToList();
        }
    }
}
