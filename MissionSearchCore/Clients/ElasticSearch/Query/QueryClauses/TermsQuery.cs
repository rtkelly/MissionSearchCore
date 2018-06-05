using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class TermsQuery : IElsQueryClause
    {
        public Dictionary<string, List<string>> terms { get; set; }

        public TermsQuery(string field, List<string> values)
        {
            terms = new Dictionary<string, List<string>>();

            terms.Add(field, values);
        }
    }
}
