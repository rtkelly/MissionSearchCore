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

        public DateRangeFacet(string fieldName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;

            Ranges = new List<DateRange>();
        }
    }


    public class DateRange
    {
        public DateTime? Lower { get; set; }
        public DateTime? Upper { get; set; }
        public string Label { get; set; }
    }
}
