using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class WildcardQuery : IElsQueryClause
    {
        public Dictionary<string, string> wildcard { get; set; }

        public  WildcardQuery(string field, string value)
        {
            wildcard = new Dictionary<string, string>();

            wildcard.Add(field, value);
        }
    }
}
