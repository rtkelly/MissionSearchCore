 using System;
using System.Collections.Generic;

namespace MissionSearch
{
    public interface ISearchDocument 
    {
        string id { get; set; }

        int sourceid { get; set; }

        string title { get; set; }
                
        string url { get; set; }
        
        List<string> content { get; set; }
        
        DateTime timestamp { get; set; }
                
        string summary { get; set; }

        string highlightsummary { get; set; }

    }
}
