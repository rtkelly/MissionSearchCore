using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class RangeQuery<T> : IElsQueryClause
    {
        public enum RangeOption
        {
            GreaterThenEqual,
            LessThenEqual
        }
        public Dictionary<string, Range<T>> range { get; set; }

        public RangeQuery(string field, T rangeStart, RangeOption rangeOption)
        {
            var rangeQuery = new Range<T>();

            if (rangeOption == RangeOption.GreaterThenEqual)
            {
                rangeQuery.gte = rangeStart;
            }

            if (rangeOption == RangeOption.LessThenEqual)
            {
                rangeQuery.lte = rangeStart;
            }


            range = new Dictionary<string, Range<T>>();

            range.Add(field, rangeQuery);
        }

        public RangeQuery(string field, T rangeStart, T rangeEnd)
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
