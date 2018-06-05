using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class PrefixQuery : IElsQueryClause
    {
        public Dictionary<string, string> prefix { get; set; }

        public PrefixQuery(string field, string value)
        {
            prefix = new Dictionary<string, string>();

            prefix.Add(field, value);

           
        }
    }
}
