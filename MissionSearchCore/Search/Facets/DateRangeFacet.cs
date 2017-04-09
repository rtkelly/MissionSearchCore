using MissionSearch.Search.Facets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class DateRangeFacet : IFacet
    {
        public string FieldName { get; set; }
        
        public string FieldLabel { get; set; }
                
        public int Order { get; set; }

        public List<DateRange> Ranges { get; set; }

        public RefinementType RefinementOption { get; set; }

        public FacetSortOption Sort { get; set; }

        public DateRangeFacet(string fieldName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            RefinementOption = RefinementType.Refinement;
            Ranges = new List<DateRange>();
            Sort = FacetSortOption.None;
        }
                
        public DateRangeFacet(string fieldName, string fieldLabel, RefinementType refinementOption)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;
            RefinementOption = refinementOption;
            Ranges = new List<DateRange>();
            Sort = FacetSortOption.None;
        }
               
        
    }


    public class DateRange
    {
        public DateTime? Lower { get; set; }
        public DateTime? Upper { get; set; }
        public string Label { get; set; }

        public DateRange()
        {
        }

        public DateRange(DateTime? lower, DateTime? upper, string label)
        {
            Lower = lower;
            Upper = upper;
            Label = label;
        }

        public DateRange(DateTime? lower, DateTime? upper, int year)
        {
            Lower = lower;
            Upper = upper;
            Label = year.ToString();
        }
    }
}
