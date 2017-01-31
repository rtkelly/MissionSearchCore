using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients
{
    public interface ICrawler
    {
        bool Crawl(string crawlId, string config);
    }
}
