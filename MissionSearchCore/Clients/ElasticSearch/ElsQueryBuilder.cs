using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MissionSearch.Clients.ElasticSearch
{
    public static class ElsQueryBuilder
    {
        public static IElsQueryRequest BuildSearchQuery(SearchRequest request)
        {
            var boolRequest = new BoolQueryRequest();

            if (!string.IsNullOrEmpty(request.QueryText))
                boolRequest.AddMust(new TermQuery("content", request.QueryText));

            boolRequest.size = request.PageSize;
            boolRequest.from = (request.CurrentPage - 1) * request.PageSize;
                       
            return boolRequest
                .AppendQueryFilters(request.QueryOptions);

        }

        private static BoolQueryRequest AppendQueryFilters(this BoolQueryRequest boolRequest, List<IQueryOption> queryOptions)
        {
            var filterQueries = new List<IElsQueryClause>();

            // term query
            filterQueries.AddRange(queryOptions
                         .OfType<FilterQuery>()
                         .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Equals)
                         .Select(qp => new TermQuery(qp.ParameterName, qp.FieldValue))
                         .ToList());

            // wildcard query
            filterQueries.AddRange(queryOptions
                         .OfType<FilterQuery>()
                         .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Contains)
                         .Select(qp => new PrefixQuery(qp.ParameterName, qp.FieldValue.ToString())));

            // Greater then date
            filterQueries.AddRange(queryOptions
                   .OfType<DateFilterQuery>()
                   .Where(fq => fq.Condition == DateFilterQuery.ConditionalTypes.GreaterThenEqual)
                   .Select(qp => new ElsRangeQuery<DateTime>(qp.ParameterName, qp.FieldValue, DateFilterQuery.ConditionalTypes.GreaterThenEqual)));

            // Less then date
            filterQueries.AddRange(queryOptions
                   .OfType<DateFilterQuery>()
                   .Where(fq => fq.Condition == DateFilterQuery.ConditionalTypes.LessThenEqual)
                   .Select(qp => new ElsRangeQuery<DateTime>(qp.ParameterName, qp.FieldValue, DateFilterQuery.ConditionalTypes.LessThenEqual)));

            // range date
            filterQueries.AddRange(queryOptions
                         .OfType<RangeQuery<DateTime>>()
                         .Select(qp => new ElsRangeQuery<DateTime>(qp.ParameterName, qp.GreaterThenValue, qp.LessThenValue)));

            if(filterQueries.Any())
            {
                boolRequest.AddFilters(filterQueries);
            }

            return boolRequest;
        }

        
    }
}
