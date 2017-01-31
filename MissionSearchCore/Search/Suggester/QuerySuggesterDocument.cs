using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Suggester
{
    public class QuerySuggesterDocument : ISearchDocument
    {
        public string id { get; set; }
        
        public List<string> content { get; set; }
        
        public string title { get; set; }
       
        public string summary { get; set; }
       
        public string url { get; set; }
       
        public string contenttype { get; set; }

        public string mimetype { get; set; }

        public List<string> language { get; set; }

        public DateTime timestamp { get; set; }

        public string hostname { get; set; }

        public int hitcount { get; set; }

        public string highlightsummary { get; set; }
                
        public int sourceid { get; set; }
        
        public List<string> paths { get; set; }


        public string parent { get; set; }
    }
}
