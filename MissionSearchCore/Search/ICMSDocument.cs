using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public interface ICMSSearchDocument : ISearchDocument
    {
        string contentid { get; set; }

        string mimetype { get; set; }

        string hostname { get; set; }
        
        List<string> paths { get; set; }

        string path { get; set; }

        List<string> categories { get; set; }

        string folder { get; set; }

        List<string> language { get; set; }
    }
}
