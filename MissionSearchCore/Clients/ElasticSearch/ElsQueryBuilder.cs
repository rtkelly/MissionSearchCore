using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class ElsQueryBuilder
    {
        public static IElsQueryRequest BuildSearchQuery(SearchRequest request)
        {
            var boolRequest = ElsQueryRequest.BoolQuery();
                        
            boolRequest.AddTerm(Els.BoolQuery.must, "content", request.QueryText);
            //boolRequest.AddTerm(Els.BoolQuery.filter, "title", request.QueryText);
            //boolRequest.AddTerm(Els.BoolQuery.mustnot, "title", "puppy");
            //boolRequest.AddDateRange(Els.BoolQuery.filter, "publication_date", DateTime.Now.AddDays(-1), DateTime.Now);

            boolRequest.size = request.PageSize;
            boolRequest.from = (request.CurrentPage-1) * request.PageSize;

            return boolRequest;

        }
    }
}
