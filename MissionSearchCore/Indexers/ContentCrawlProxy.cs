using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Indexers
{
    /// <summary>
    /// 
    /// </summary>
    public class CrawlerContent
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ContentCrawlProxy
    {
        private Dictionary<string, object> CrawlSettings { get; set; }

        public List<CrawlerContent> Content { get; set; }
        
        public ISearchableContent ContentItem { get; set; }
        
        public ContentCrawlProxy(Dictionary<string, object> settings=null)
        {
            CrawlSettings = settings;
            Content = new List<CrawlerContent>();
        }
        
    }
}
