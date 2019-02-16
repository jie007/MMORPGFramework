using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.RestApi;
using Newtonsoft.Json;

namespace CommonServer.Documents
{
    public class UserInformation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string PasswordHash { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
