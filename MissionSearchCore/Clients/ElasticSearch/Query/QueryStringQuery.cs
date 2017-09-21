using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class QueryStringQueryRequest : IElsQueryRequest
    {
        public string default_field { get; set; }

        public ElsQuery query { get; set; }

        public string nullvalue { get; set; }

        public int from { get; set; }

        public int size { get; set; }

    }


}


