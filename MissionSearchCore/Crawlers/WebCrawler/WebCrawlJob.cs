using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
    public class WebCrawlJob
    {
        public string Name { get; set; }
        
        public string SeedUrl { get; set; }
        
        public List<string> CrawlUrlPattern { get; set; }
        
        public List<string> CrawlSkipUrlPattern { get; set; }
        
        public List<string> IndexUrlPattern { get; set; }
        
        public List<string> IndexSkipUrlPattern { get; set; }
        
        public List<string> LinkCleanupPattern { get; set; }
                
        public List<string> Metadata { get; set; }
        
        public int Depth { get; set; }

        public int MaxLinkCnt { get; set; }

        public int SourceId { get; set; }

        public string TitlePattern { get; set; }
        
        public string SummaryPattern { get; set; }
        
        public List<string> ContentPattern { get; set; }
        
        public List<string> MetadataPattern { get; set; }

        
    }
}
