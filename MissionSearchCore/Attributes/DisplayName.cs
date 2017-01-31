using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class DisplayName : Attribute
    {
        public String FieldName { get; set; }

        public DisplayName(string name)
        {
            FieldName = name;
        }
    }
}
