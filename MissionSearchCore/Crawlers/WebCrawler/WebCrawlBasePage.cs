using MissionSearch.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
    public class WebCrawlBasePage : IWebCrawlPage
    {
        //[MapHtmlNode("//body//h1")]
        public string Name { get; set; }
                
        //[MapHtmlNode("//p[@class='fl']")]
        public DateTime Changed { get; set; }
        
        [SearchIndex("summary")]
        public string Summary { get; set; }

        [SearchIndex("hostname")]
        public string Hostname { get; set; }

        [SearchIndex("contenttype")]
        public string ContentType { get { return "External Content"; } }
        
        [SearchIndex]
        public List<string> Content { get; set; }

        public string _ContentID { get; set; }
        
        public string SearchUrl { get; set; }
        
        public object CrawlProperties { get; set; }
        
        public bool NotSearchable { get; set; }

    }
}
