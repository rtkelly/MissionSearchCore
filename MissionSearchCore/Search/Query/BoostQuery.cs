using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Search.Query
{
    public class BoostQuery : IQueryOption
    {
        public string ParameterName { get; set; }
        public string FieldValue { get; set; }
        public double Boost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        public BoostQuery(string fieldName, string fieldValue, double boost)
        {
            ParameterName = fieldName;
            FieldValue = fieldValue;
            Boost = boost;
        }
                
        
    }
}
