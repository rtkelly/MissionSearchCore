using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class BoolQueryRequest : QueryRequstBase
    {
        public BoolQueryRequest()
        {
            query = new ElsQuery();
            query.bool_query = new BoolQueries();
        }

        public void AddMust(IElsQueryClause newQuery)
        {
            if (query.bool_query.must == null)
                query.bool_query.must = new List<IElsQueryClause>();

            query.bool_query.must.Add(newQuery);
        }

        public void AddMustNot(IElsQueryClause newQuery)
        {
            if (query.bool_query.must_not == null)
                query.bool_query.must_not = new List<IElsQueryClause>();

            query.bool_query.must_not.Add(newQuery);
        }

        public void AddShould(IElsQueryClause newQuery)
        {
            if (query.bool_query.should == null)
                query.bool_query.should = new List<IElsQueryClause>();

            query.bool_query.should.Add(newQuery);
        }

        public void AddFilter(IElsQueryClause newQuery)
        {
            if (query.bool_query.filter == null)
                query.bool_query.filter = new List<IElsQueryClause>();

            query.bool_query.filter.Add(newQuery);
        }

        public void AddFilters(IEnumerable<IElsQueryClause> newQuery)
        {
            if (query.bool_query.filter == null)
                query.bool_query.filter = new List<IElsQueryClause>();

            query.bool_query.filter.AddRange(newQuery);
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
