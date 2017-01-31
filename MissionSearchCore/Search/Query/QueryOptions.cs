using MissionSearch.Search.Query;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class QueryOptions 
    {
                        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="refinements"></param>
        /// <returns></returns>
        public static List<FilterQuery> ParseRefinementString(string refinements)
        {
            var list = new List<FilterQuery>();
           
            if (string.IsNullOrEmpty(refinements))
                return list;

            var decodedRefinements = StringEncoder.DecodeString(refinements);

            foreach (var r in decodedRefinements.Split(','))
            {
                var fields = r.Split(';');

                if (fields.Count() == 3)
                {
                    list.Add(new FilterQuery(fields[0], fields[1]));
                }
            }

            return list;
        }
               

      
    }
}
