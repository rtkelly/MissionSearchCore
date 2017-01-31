using MissionSearch.Search.Facets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class NumRangeFacet : IFacet
    {
        public enum FormatType
        {
            Currency = 0,
            Numeric = 1,
            Decimal = 2,
            Percentage = 3,
        }

       

        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public List<NumRange> Range { get; set; }
        public FormatType NumericFormat { get; set; }
        public int Order { get; set; }

        public NumRangeFacet(string fieldName, string fieldLabel)
        {
            FieldName = fieldName;
            FieldLabel = fieldLabel;

            Range = new List<NumRange>();
            NumericFormat = FormatType.Numeric;
        }
    }

    public class NumRange
    {
        public double? Lower { get; set; }
        public double? Upper { get; set; }
        
    }
}
