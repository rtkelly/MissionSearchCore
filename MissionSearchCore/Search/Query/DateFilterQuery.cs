using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class DateFilterQuery : IQueryOption
    {
        public enum ConditionalTypes
        {
            GreaterThenEqual = 1,
            LessThenEqual = 2,
        }

        public string ParameterName { get; set; }
        
        public DateTime FieldValue { get; set; }
                
        public ConditionalTypes Condition { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        public DateFilterQuery(string fieldName, ConditionalTypes condition, DateTime fieldValue)
        {
            ParameterName = fieldName;
            FieldValue = fieldValue;
            Condition = condition;
        }

        
    }
}
