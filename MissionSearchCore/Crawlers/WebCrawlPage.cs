using MissionSearch.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
    public class WebCrawlPage : ISearchableContent
    {
        public string SearchId { get; set; }

        public string Name { get; set; }

        public string SearchUrl { get; set; }

        public DateTime Changed { get; set; }

        public object CrawlProperties { get; set; }

        public bool NotSearchable { get; set; }

        [SearchIndex("summary")]
        public string Summary { get; set; }

        [SearchIndex("hostname")]
        public string Hostname { get; set; }
        
        [SearchIndex]
        public string Content { get; set; }
    }
}
