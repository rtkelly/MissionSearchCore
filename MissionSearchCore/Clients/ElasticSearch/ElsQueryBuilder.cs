using MissionSearch.Search.Query;
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
            var boolRequest = new BoolQueryRequest();

            if (!string.IsNullOrEmpty(request.QueryText))
                boolRequest.AddMust(new TermQuery("content", request.QueryText));

            boolRequest.size = request.PageSize;
            boolRequest.from = (request.CurrentPage - 1) * request.PageSize;

            boolRequest = AppendQueryOptions(boolRequest, request.QueryOptions);

            return boolRequest;

        }

        private static BoolQueryRequest AppendQueryOptions(BoolQueryRequest boolRequest, List<IQueryOption> queryOptions)
        {
            var filterEqualQueries = queryOptions
                           .OfType<FilterQuery>()
                           .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Equals)
                           .Select(qp => new TermQuery(qp.ParameterName, qp.FieldValue))
                           .ToList();

            if(filterEqualQueries.Any())
            {
                if (boolRequest.query.bool_query.filter == null)
                    boolRequest.query.bool_query.filter = new List<IElsQueryClause>();

                boolRequest.query.bool_query.filter.AddRange(filterEqualQueries);
            }

            var filterWildcardQueries = queryOptions
                          .OfType<FilterQuery>()
                          .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Contains)
                          .Select(qp => new PrefixQuery(qp.ParameterName, qp.FieldValue.ToString()));

            if (filterWildcardQueries != null && filterWildcardQueries.Any())
            {
                if (boolRequest.query.bool_query.filter == null)
                    boolRequest.query.bool_query.filter = new List<IElsQueryClause>();

                boolRequest.query.bool_query.filter.AddRange(filterWildcardQueries);
            }


            return boolRequest;
        }
    }
}
