using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class ElsQueryRequest
    {
      
        public ElsQuery query { get; set; }

        public static ElsQueryRequest Create()
        {
            var c = new ElsQueryRequest();
            c.query = new ElsQuery();
                
            
            return c;
        }


        public static BoolQueryRequest BoolQuery()
        {
            var c = new BoolQueryRequest();
            c.query = new ElsQuery();

            c.query.bool_query = new BoolQueries();
            /*
            c.query.bool_query.must = new List<IElsQueryClause>();
            c.query.bool_query.must_not = new List<IElsQueryClause>();
            c.query.bool_query.should = new List<IElsQueryClause>();
            c.query.bool_query.filter = new List<IElsQueryClause>();
            */
            return c;
        }

    }

    public class ElsQuery
    {
      

        public QueryStringQueryRequest query_string { get; set; }

        public BoolQueries bool_query { get; set; }
        
        
    }
}
