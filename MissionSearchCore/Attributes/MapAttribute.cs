using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class MapAttribute : Attribute
    {
        public MapAttribute(string xpath, string attributeName)
        {
            XPath = xpath;
            AttributeName = attributeName;

        }

        public String XPath { get; set; }
        public String AttributeName { get; set; }
    }
}
