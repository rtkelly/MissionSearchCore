using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class ElsQueryBuilder
    {
        public static QueryStringQuery BuildSearchQuery(SearchRequest request)
        {
            
            var query = new QueryStringQuery();

            query.query = new QueryStringQuery.Query();

            query.query.query_string = new QueryString()
            {
                default_field = "content",
                query = request.QueryText,
            };
            
            /*
            var query = new BoolQuery();

            query.query = new BoolQuery.Query();

            query.query.bool_query = new BoolQueries();

            query.query.bool_query.must = new Dictionary<string, KeyValuePair<string, string>>();

            var termQuery = new KeyValuePair<string, string>("content", request.QueryText);
            query.query.bool_query.must.Add("term", termQuery);

            //query.query.query_bool = new BoolQuery();
            */
            return query;
        }
    }
}
