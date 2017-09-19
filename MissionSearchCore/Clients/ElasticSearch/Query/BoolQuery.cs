using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{

    public class BoolQuery : IElsQuery
    {
        public IQuery query { get; set; }

        public class Query : IQuery
        {
            public BoolQueries bool_query { get; set; }

        }
    }

    public class BoolQueries
    {
        public Dictionary<string, KeyValuePair<string, string>> must { get; set; }

        public Dictionary<string, KeyValuePair<string, string>> must_not { get; set; }

        public Dictionary<string, KeyValuePair<string, string>> filter { get; set; }

        public Dictionary<string, KeyValuePair<string, string>> should { get; set; }
    }

   
}
