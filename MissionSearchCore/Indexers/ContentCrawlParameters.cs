using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Indexers
{

    public class CrawlerContent
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class ContentCrawlParameters
    {
        private Dictionary<string, object> CrawlSettings { get; set; }

        public List<CrawlerContent> Content { get; set; }

        //public string ContentId { get; set; }

        public ISearchableContent ContentItem { get; set; }
        
        public ContentCrawlParameters(Dictionary<string, object> settings=null)
        {
            CrawlSettings = settings;
            Content = new List<CrawlerContent>();
        }

               

        public bool EnablePageScrape
        {
            get
            {
                if (CrawlSettings == null)
                    return false;

                if (CrawlSettings.ContainsKey("EnablePageScrape"))
                {
                    if (CrawlSettings["EnablePageScrape"] is bool)
                    {
                        var b = CrawlSettings["EnablePageScrape"] as bool?;

                        if (b == null)
                            return false;

                        return b.Value;
                    }
                }
                return false;
            }
        }
    }
}
