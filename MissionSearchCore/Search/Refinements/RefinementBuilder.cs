using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MissionSearch.Search.Refinements
{
    public static class RefinementBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentRefinementsStr"></param>
        /// <param name="refinement"></param>
        /// <param name="refinementType"></param>
        /// <returns></returns>
        public static string AddRemoveRefinement(RefinementItem refinement, string currentRefinementsStr, RefinementType refinementType)
        {
            var refinementStr = string.Format("{0};{1};{2}", refinement.Name, refinement.Value, refinement.GroupLabel);

            if (string.IsNullOrEmpty(currentRefinementsStr))
            {
                return StringEncoder.EncodeString(refinementStr);
            }

            var decodedCurrentRefinements = StringEncoder.DecodeString(currentRefinementsStr) ?? "";

            var currentRefinements = decodedCurrentRefinements.Split(',').ToList();

            var currentRefinementItems = currentRefinements.Select(r => new RefinementItem(r)).ToList();

            switch (refinementType)
            {
                case RefinementType.Single_Select:

                    if (decodedCurrentRefinements.Contains(refinementStr))
                    {
                        return StringEncoder.EncodeString(string.Join(",", currentRefinements.Where(p => p != refinementStr)));
                    }
                    else if (decodedCurrentRefinements.Contains(refinement.GroupLabel))
                    {
                        decodedCurrentRefinements = string.Join(",", currentRefinements.Where(p => !p.Contains(refinement.GroupLabel)));

                        return StringEncoder.EncodeString(string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
                    }

                    break;

                case RefinementType.Multi_Select:

                    var likeRefinement = currentRefinementItems.FirstOrDefault(p => p.GroupLabel == refinement.GroupLabel);

                    if (likeRefinement != null)
                    {
                        //var rawValues = likeRefinement.Value.Replace("(", "").Replace(")", "");
                        var rawValues = likeRefinement.Value.StartsWith("(") ? likeRefinement.Value.Substring(1, likeRefinement.Value.Length - 2) : likeRefinement.Value;
                        
                        var values = Regex.Split(rawValues, " OR ");

                        var valueStr = string.Join(" OR ", values.Where(v => v != refinement.Value));

                        if (values.Any(v => v == refinement.Value))
                        {
                            refinementStr = (string.IsNullOrEmpty(valueStr)) ? "" : string.Format("{0};({1});{2}", refinement.Name, valueStr, refinement.GroupLabel);
                        }
                        else
                        {
                            refinementStr = string.Format("{0};({1} OR {2});{3}", refinement.Name, valueStr, refinement.Value, refinement.GroupLabel);
                        }

                        decodedCurrentRefinements = string.Join(",", currentRefinements.Where(p => !p.Contains(refinement.GroupLabel)));

                        //return StringEncoder.EncodeString(string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
                    }

                    //return (string.IsNullOrEmpty(decodedCurrentRefinements)) ? refinementStr : StringEncoder.EncodeString(string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
                    break;
                default:

                    if (decodedCurrentRefinements.Contains(refinementStr))
                    {
                        return StringEncoder.EncodeString(string.Join(",", currentRefinements.Where(p => p != refinementStr)));
                    }

                    break;
            }

            //return StringEncoder.EncodeString(string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
            return StringEncoder.EncodeString(string.IsNullOrEmpty(decodedCurrentRefinements) ? refinementStr : string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
                    


        }
        

    }
}
