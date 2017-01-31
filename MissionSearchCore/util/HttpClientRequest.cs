using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public class HttpClientRequest
    {
        public string EndPoint { get; set; }
        public string Method { get; set; }
        public string Referrer { get; set; }
        public int Timeout { get; set; }

    }
}
