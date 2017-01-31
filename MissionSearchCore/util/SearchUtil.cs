using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public class SearchUtil
    {
        /*
        public static List<string> GetAllFacets<T>(string srchField) where T : ISearchDocument
        {
            var facets = new List<IFacet>();

            facets.Add(new FieldFacet(srchFieldName));

            var resp = SearchFactory<T>.SearchClient.Search(new SearchRequest()
            {
                QueryText = "*",
                PageSize = 1,
                SingleSelectRefinement = true,
                Facets = facets,
            });

            var refinement = resp.Refinements.FirstOrDefault(r => r.Name == srchFieldName);
            var items = (refinement != null) ? refinement.Items : new List<RefinementItem>();

            var categories = new List<string>();

            items.ForEach(item => categories.Add(item.Value.Replace("\"", "")));
        }
         * */
    }
}
