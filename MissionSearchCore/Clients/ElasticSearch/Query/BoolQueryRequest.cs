using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class BoolQueryRequest : IElsQueryRequest
    {
        public int from { get; set; }

        public int size { get; set; }

        public ElsQuery query { get; set; }

        public void AddMatch(Els.BoolQuery queryType, string field, string value)
        {
            var match = new Dictionary<string, string>();

            match.Add(field, value);

            var matchQuery = new MatchQuery()
            {
                match = match,
            };

            Add(queryType, matchQuery);
        }

        public void AddTerm(Els.BoolQuery queryType, string field, string value)
        {
            var term = new Dictionary<string, string>();

            term.Add(field, value);

            var termQuery = new TermQuery()
            {
                term = term,
            };

            Add(queryType, termQuery);
        }

        private void Add(Els.BoolQuery queryType, IElsQueryClause newQuery)
        { 
            switch (queryType)
            {
                case Els.BoolQuery.must:

                    if (query.bool_query.must == null)
                        query.bool_query.must = new List<IElsQueryClause>();

                    query.bool_query.must.Add(newQuery);

                    break;

                case Els.BoolQuery.mustnot:

                    if (query.bool_query.must_not == null)
                        query.bool_query.must_not = new List<IElsQueryClause>();

                    query.bool_query.must_not.Add(newQuery);

                    break;

                case Els.BoolQuery.should:

                    if (query.bool_query.should == null)
                        query.bool_query.should = new List<IElsQueryClause>();

                    query.bool_query.should.Add(newQuery);

                    break;

                case Els.BoolQuery.filter:

                    if (query.bool_query.filter == null)
                        query.bool_query.filter = new List<IElsQueryClause>();

                    query.bool_query.filter.Add(newQuery);

                    break;

            }



        }

        public void AddRange(Els.BoolQuery queryType, string field, long rangeStart, long rangeEnd)
        {
            /*
            var term = new Dictionary<string, string>();

            term.Add(field, value);

            if (query.bool_query.must == null)
                query.bool_query.must = new List<IElsQueryClause>();

            query.bool_query.must.Add(new TermQuery()
            {
                term = term,
            });
            */
        }

        public void AddDateRange(Els.BoolQuery queryType, string field, DateTime rangeStart, DateTime rangeEnd)
        {
            /*
            var term = new Dictionary<string, string>();

            term.Add(field, value);

            if (query.bool_query.must == null)
                query.bool_query.must = new List<IElsQueryClause>();

            query.bool_query.must.Add(new TermQuery()
            {
                term = term,
            });
            */
        }
    }

    public class BoolQueries
    {
        
        public List<IElsQueryClause> must { get; set; }

        public List<IElsQueryClause> must_not { get; set; }

        public List<IElsQueryClause> filter { get; set; }

        public List<IElsQueryClause> should { get; set; }

        
    }

   
}
