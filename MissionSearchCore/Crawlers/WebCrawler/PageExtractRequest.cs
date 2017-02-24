using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
    public class PageExtractRequest
    {
        public string PageUrl { get; set; }

        public IWebCrawlPage PageModel { get; set; }

        public string TitlePattern { get; set; }

        public string SummaryPattern { get; set; }

        public List<string> ContentPattern { get; set; }

        public List<string> MetadataPattern { get; set; }
    }
}
