using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class PaginationPage
    {
        public int Page { get; set; }
        public string Link { get; set; }
        public bool Current { get; set; }
    }
}
