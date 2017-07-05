using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;

namespace MissionSearch.Indexers
{
    public class IndexResults
    {
        public int TotalCnt { get; set; }

        public int DeleteCnt { get; set; }
        
        public int ErrorCnt { get; set; }

        public List<long> CrawledFoldersIds { get; set; }
        
        public int WarningCnt { get; set; }

        public TimeSpan Duration { get; set; }

        public bool Stopped { get; set; }

        public IndexResults()
        {
            CrawledFoldersIds = new List<long>();    
        }

        public IndexResults Combine(IndexResults results1)
        {
            TotalCnt += results1.TotalCnt;
            ErrorCnt += results1.ErrorCnt;

            if (results1.CrawledFoldersIds != null && results1.CrawledFoldersIds.Any())
            {
                CrawledFoldersIds.AddRange(results1.CrawledFoldersIds);
            }



            return this;
        }
    }
}
