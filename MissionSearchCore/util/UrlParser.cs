using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public class UrlParser
    {

        public static string GetHostName(string url)
        {
           var uri = new Uri(url);
           return uri.Host;  
        }

        public static string GetHostandPath(string url)
        {
            var uri = new Uri(url);
            return uri.Host + uri.PathAndQuery;
        }

        public static string GetSchema(string url)
        {
            var uri = new Uri(url);
            return uri.Scheme;
        }
    }
}
