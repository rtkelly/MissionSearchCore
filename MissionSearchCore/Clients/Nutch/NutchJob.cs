using System.Collections.Generic;

namespace MissionSearch.Clients.Nutch
{
    public class NutchJob
    {
        public string crawlId { get; set; }
        public string type { get; set; }
        public string confId { get; set; }
        public Dictionary<string, string> args { get; set; }
    }


    //{"id":"crawl1-default-INJECT-1873093519","type":"INJECT","confId":"default","args":{"url_dir":"urls"},"result":null,"state":"RUNNING","msg":"OK","crawlId":"crawl1"}

    public class NutchResults
    {
        public string id { get; set; }
        public string type { get; set; }
        public string confId { get; set; }
        //public List<string> args { get; set; }
        //public string result { get; set; }
        public string state { get; set; }
        public string msg { get; set; }
        public string crawlId { get; set; }
    }

    public class SeedList
    {
        public string name { get; set; }
        public string[] seedUrls { get; set; }
    }


}
