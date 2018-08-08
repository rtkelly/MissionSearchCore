using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class ElsRangeQuery<T> : IElsQueryClause
    {
       
        public Dictionary<string, Range<T>> range { get; set; }

        public ElsRangeQuery(string field, T rangeStart, DateFilterQuery.ConditionalTypes rangeOption)
        {
            var rangeQuery = new Range<T>();

            if (rangeOption == DateFilterQuery.ConditionalTypes.GreaterThenEqual)
            {
                rangeQuery.gte = rangeStart;
            }

            if (rangeOption == DateFilterQuery.ConditionalTypes.LessThenEqual)
            {
                rangeQuery.lte = rangeStart;
            }


            range = new Dictionary<string, Range<T>>();

            range.Add(field, rangeQuery);
        }

        public ElsRangeQuery(string field, T rangeStart, T rangeEnd)
        {
            var rangeQuery = new Range<T>()
            {
                gte = rangeStart,
                lte = rangeEnd,
            };

            range = new Dictionary<string, Range<T>>();

            range.Add(field, rangeQuery);
        }
    }

    public class Range<T>
    {
        public T gte { get; set; }
        public T lte { get; set; }
        
    }
}
