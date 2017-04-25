using MissionSearch.Search.Facets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public enum FacetSortOption
    {
        None = 0,
        Count = 1,
        Name = 2,
        NameDesc = 3,
    }

    public class FieldFacet : IFacet
    {

        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public int Order { get; set; }
        public FacetSortOption Sort { get; set; }
        public RefinementType RefinementOption { get; set; }
        


        public FieldFacet(string fieldName)
        {
            FieldName = fieldName;
            FieldLabel = fieldName;
            Sort = FacetSortOption.Count;
            RefinementOption = RefinementType.Refinement;
        }
                
        public FieldFacet(string fieldName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            Sort = FacetSortOption.Count;
            RefinementOption = RefinementType.Refinement;
        }

        public FieldFacet(string fieldName, string fieldLabel, RefinementType refinementOption)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            Sort = FacetSortOption.Count;
            RefinementOption = refinementOption;
        }



    }
}
