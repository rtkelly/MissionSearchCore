﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{


    public class ElsQuery
    {
      

        public QueryStringRequest query_string { get; set; }

        public BoolQueries bool_query { get; set; }
        
        
    }
}
