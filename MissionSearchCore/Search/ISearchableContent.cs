using System;
using System.Collections.Generic;
using System.Globalization;

namespace MissionSearch
{

    public interface ISearchableContent : ISearchable
    {
        String SearchId { get; set; }

        String Name { get; set;  }

        String SearchUrl { get; set; }
  
        DateTime Changed { get; set; }
              
        object CrawlProperties { get; set; }

       
                
    }
}
