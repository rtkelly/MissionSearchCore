using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Search.Query
{
    public class BoostFunction
    {
        public string FieldName { get; set; }
        public double Boost { get; set; }
    }
}
