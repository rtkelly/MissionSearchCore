using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class TermQuery : IElsQueryClause
    {
        public Dictionary<string, object> term { get; set; }

        public TermQuery(string field, object value)
        {
            term = new Dictionary<string, object>();
            term.Add(field, value);
        }

       
    }
}
