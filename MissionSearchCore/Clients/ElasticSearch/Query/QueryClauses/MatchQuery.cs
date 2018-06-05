using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class MatchQuery : IElsQueryClause
    {
        public Dictionary<string, string> match { get; set; }

        public MatchQuery(string field, string value)
        {
            match = new Dictionary<string, string>();

            match.Add(field, value);
                       
        }
    }
}
