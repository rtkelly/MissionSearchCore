using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Crawlers
{
    public interface ICrawler
    {
        CrawlerResults Run();
    }
}
