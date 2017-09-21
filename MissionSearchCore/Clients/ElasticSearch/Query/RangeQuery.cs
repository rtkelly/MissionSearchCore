using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class RangeQuery : IElsQueryClause
    {
        /*
         "range" : {
          "age" : { "gte" : 10, "lte" : 20 }
        }
         * */
        public Dictionary<string, Range> range { get; set; }
    }

    public class Range
    {
        long gte { get; set; }
        long lte { get; set; }
        
    }
}
