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

    public class CrawlerContentSettings
    {
        private Dictionary<string, object> CrawlSettings { get; set; }

        public List<CrawlerContent> Content { get; set; }

        public CrawlerContentSettings(Dictionary<string, object> settings)
        {
            CrawlSettings = settings;
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
