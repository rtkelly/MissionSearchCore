using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
    public class CrawlerResults
    {
        public int SourceId { get; set; }

        public int TotalCnt { get; set; }

        public int CrawledCnt { get; set; }

        public int IndexedCnt { get; set; }

        public int ErrorCnt { get; set; }

        public int WarningCnt { get; set; }

        public TimeSpan Duration { get; set; }

        public bool Stopped { get; set; }

        public List<IWebCrawlPage> CrawlPages { get; set; }
             
    }
}
