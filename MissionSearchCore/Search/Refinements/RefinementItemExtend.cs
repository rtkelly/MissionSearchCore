using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Search
{
    public static class RefinementItemExtend
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentRefinements"></param>
        /// <param name="refinement"></param>
        /// <returns></returns>
        /*
        public static string AddRemoveRefinement(this RefinementItem refinement, string currentRefinements)
        {
            var refinementStr = string.Format("{0};{1}", refinement.Name, refinement.Value);

            if (string.IsNullOrEmpty(currentRefinements))
            {
                return refinementStr;
            }

            if (currentRefinements.Contains(refinementStr))
            {
                return string.Join(",", currentRefinements.Split(',').Where(p => p != refinementStr));
            }
            else
            {
                return string.Format("{0},{1}", currentRefinements, refinementStr);
            }
        }
        */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="facet"></param>
        /// <param name="currentRefinements"></param>
        /// <returns></returns>
        /*
        public static string AddRemoveRefinement(this RefinementItem facet, IEnumerable<IQueryOption> currentRefinements)
        {
            var refinementStr = string.Format("{0};{1}", facet.Name, facet.Value);

            if (!currentRefinements.Any())
            {
                return refinementStr;
            }

            if (currentRefinements.Any(r => r.FieldName == facet.Name))
            {
                return string.Join(",", currentRefinements.Where(p => p.FieldName != facet.Name)
                    .Select(p => string.Format("{0};{1}", p.FieldName, p.FieldValue)));
            }
            else
            {
                var currentRefStr = string.Join(",", currentRefinements.Select(p => string.Format("{0};{1}", p.FieldName, p.FieldValue)));

                return string.Format("{0},{1}", currentRefStr, refinementStr);
            }
        }
         */
         

    }
}
