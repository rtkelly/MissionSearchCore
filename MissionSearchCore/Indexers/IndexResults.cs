using System;
using System.Collections.Generic;

namespace MissionSearch.Indexers
{
    public class IndexResults
    {
        public int TotalCnt { get; set; }

        public int DeleteCnt { get; set; }
        
        public int ErrorCnt { get; set; }

        public int WarningCnt { get; set; }

        public TimeSpan Duration { get; set; }

        public bool Stopped { get; set; }
            
        public IndexResults Combine(IndexResults results1)
        {
            TotalCnt += results1.TotalCnt;
            ErrorCnt += results1.ErrorCnt;

            return this;
        }
    }
}
