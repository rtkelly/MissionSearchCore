using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class QueryStringQuery
    {
        public Query query { get; set; }

        public class Query
        {
            public QueryString query_string { get; set; }

        }
    }
    

    public class QueryString
    {
        public string default_field { get; set; }

        public string query { get; set; }

        public string nullvalue { get; set; }

    }


}


