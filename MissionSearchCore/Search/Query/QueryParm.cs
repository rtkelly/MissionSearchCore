using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class QueryParm : IQueryOption
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        
        public QueryParm(string parmName, string parmValue)
        {
            ParameterName = parmName;
            ParameterValue = parmValue;
        }
    }
}
