using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
   public interface IWebCrawlPage : ISearchableContent
    {
       string _ContentID { get; set; }

       string Name { get; set; }

       string SearchUrl { get; set; }

       DateTime Changed { get; set; }

       object CrawlProperties { get; set; }

       bool NotSearchable { get; set; }

       string Summary { get; set; }

       string Hostname { get; set; }

       List<string> Content { get; set; }
    }
}
