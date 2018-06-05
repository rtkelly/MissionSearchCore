using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class TermAgg : IAgg
    {
        public Dictionary<string, string> terms { get; set; }
    }
}
