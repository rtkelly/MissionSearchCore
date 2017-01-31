using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Indexers
{
    class ContentIndexRequest
    {
        public ISearchableContent Content { get; set; }
        public string PageScrap { get; set; }
    }
}
