using MissionSearch.Search.Facets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class CategoryFacet : IFacet
    {
        public string FieldName { get; set; }
        public string CategoryName { get; set; }
        public string FieldLabel { get; set; }

        public int Order { get; set; }
        public FacetSortOption Sort { get; set; }
        public RefinementType RefinementOption { get; set; }
        
        public CategoryFacet(string fieldName, string categoryName)
        {
            FieldName = fieldName;
            FieldLabel = fieldName;
            CategoryName = categoryName;
            Sort = FacetSortOption.Count;
            RefinementOption = RefinementType.Refinement;
        }

        public CategoryFacet(string fieldName, string categoryName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            CategoryName = categoryName;
            Sort = FacetSortOption.Count;
            RefinementOption = RefinementType.Refinement;
        }

        public CategoryFacet(string fieldName, string categoryName, string fieldLabel, RefinementType refinementOption)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            CategoryName = categoryName;
            Sort = FacetSortOption.Count;
            RefinementOption = refinementOption;
        }
    }
}
