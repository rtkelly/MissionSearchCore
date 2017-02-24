using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
   public interface IWebCrawlPage : ISearchableContent
    {
       string SearchUrl { get; set; }

       DateTime Changed { get; set; }

       object CrawlProperties { get; set; }
              
       string Summary { get; set; }

       string Hostname { get; set; }

       List<string> Content { get; set; }
    }
}
