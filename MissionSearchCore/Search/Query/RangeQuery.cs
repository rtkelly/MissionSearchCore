using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Search.Query
{
    public class RangeQuery<T> : IQueryOption
    {
        public string ParameterName { get; set; }

        public T GreaterThenValue { get; set; }

        public T LessThenValue { get; set; }

        public RangeQuery(string parmName, T gte, T lte)
        {
            ParameterName = parmName;
            GreaterThenValue = gte;
            LessThenValue = lte;
        }
    }
}
