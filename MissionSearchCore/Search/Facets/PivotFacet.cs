using MissionSearch.Search.Facets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class PivotFacet : IFacet
    {
        public string FieldName { get; set; }

        public string FieldLabel { get; set; }

        public int facetLimit { get; set; }

        public int Order { get; set; }

        public RefinementType RefinementOption { get; set; }

        public FacetSortOption Sort { get; set; }

        public PivotFacet(string fieldName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            Sort = FacetSortOption.Count;
            RefinementOption = RefinementType.Refinement;
            facetLimit = -1;
        }
    }
}
