using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServer.Documents;

namespace CommonServer.DocDb
{
    public class UserInformationRepository : DocDbRepository<UserInformation>
    {
        public UserInformationRepository() : base(DocumentDbConfiguration.DocumentDb, DocumentDbConfiguration.DocumentDbLoginDbCollection)
        {

        }
    }
}
