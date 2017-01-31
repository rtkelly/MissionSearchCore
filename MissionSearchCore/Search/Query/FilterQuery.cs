using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class FilterQuery : IQueryOption
    {
        public enum ConditionalTypes
        {
            Equals = 0,
            NotEqual = 3,
            Contains = 4,
        }

        
        
        public string ParameterName { get; set; }
        
        public object FieldValue { get; set; }
        
        public ConditionalTypes Condition { get; set; }
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        public FilterQuery(string fieldName, object fieldValue)
        {
            ParameterName = fieldName;
            FieldValue = fieldValue;
            Condition = ConditionalTypes.Equals;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="conditionalType"></param>
        /// <param name="fieldValue"></param>
        public FilterQuery(string fieldName, ConditionalTypes conditionalType, object fieldValue)
        {
            ParameterName = fieldName;
            FieldValue = fieldValue;
            Condition = conditionalType;
        }

               

       
    }
}
