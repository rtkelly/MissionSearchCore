using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class Pagination
    {
        public string FirstPageLink { get; set; }
        public string LastPageLink { get; set; }
        public string NextPageLink { get; set; }
        public string PrevPageLink { get; set; }
        public int PrevPage { get; set; }
        public int NextPage { get; set; }
        public int FirstPage { get; set; }
        public int LastPage { get; set; }

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public int TotalRows { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        
        public List<PaginationPage> Pages { get; set; }
    }
}
