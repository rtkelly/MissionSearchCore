using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch 
{
    public class QueryRequstBase : IElsQueryRequest
    {
        public int from { get; set; }

        public int size { get; set; }

        public ElsQuery query { get; set; }

        public List<Dictionary<string, IAgg>> aggs { get; set; }

        public string GetJsonQuery()
        {
            var jsonDoc = JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            });

            jsonDoc = jsonDoc.Replace("bool_query", "bool");


            return jsonDoc;
        }
    }
}
