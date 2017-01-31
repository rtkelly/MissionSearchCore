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
        Count = 0,
        Name = 1,
    }

    public class FieldFacet : IFacet
    {

        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public int Order { get; set; }
        public FacetSortOption Sort { get; set; }
        public RefinementTypes RefinementOption { get; set; }
        


        public FieldFacet(string fieldName)
        {
            FieldName = fieldName;
            FieldLabel = fieldName;
            Sort = FacetSortOption.Count;
        }

        public FieldFacet(string fieldName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            Sort = FacetSortOption.Count;
        }


    }
}
