using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class MapHtmlNode : Attribute
    {

        public MapHtmlNode(string xpath)
        {
            XPath = xpath;

        }

        public String XPath { get; set; }
    }
}
