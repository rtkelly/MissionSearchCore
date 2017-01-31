using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MissionSearch
{
    public enum RefinementTypes
    {
        SingleSelect,
        MultiSelect,
        Refinement
    }

    public class Refinement
    {
        public string Label { get; set; }
                
        public string Name { get; set; }
                
        public List<RefinementItem> Items { get; set; }

        public Refinement()
        {
            Items = new List<RefinementItem>();
        }
    }
}