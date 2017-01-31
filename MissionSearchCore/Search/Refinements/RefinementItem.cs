using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MissionSearch
{
    public class RefinementItem
    {
        public string Name { get; set; }
        
        public string Value { get; set; }
        
        public string Label { get; set; }
        
        public long Count { get; set; }
        
        public bool Selected { get; set; }
        
        public string Refinement { get; set; }

        public string Link { get; set; }

        public RefinementItem()
        {
        }

        public RefinementItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public RefinementItem(string  value)
        {
            var values = value.Split(';');

            if(values.Count() == 2)
            {
                Name = values[0];
                Value = values[1];
            }
        }
    }
}