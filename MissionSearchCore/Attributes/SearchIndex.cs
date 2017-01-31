using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SearchIndex : Attribute
    {

        public SearchIndex()
        {

        }

        public SearchIndex(string fieldName)
        {
            FieldName = fieldName;

        }

        public String FieldName { get; set; }
    }

    
}
