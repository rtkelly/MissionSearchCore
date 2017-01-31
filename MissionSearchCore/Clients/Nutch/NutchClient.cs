using MissionSearch.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MissionSearch.Clients.Nutch
{
    public class NutchClient : ICrawler
    {
        string _nutchConnStr;
        int _topN;
        string _urlDir;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchConnectionString"></param>
        public NutchClient(string srchConnectionString, string urlDir="urls", int topN=10)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Connection string undefined");

            _nutchConnStr = srchConnectionString;
            _topN = topN;
            _urlDir = urlDir;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="crawlId"></param>
        /// <param name="confId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public NutchResults RunJob(string jobType, string crawlId, string confId, Dictionary<string, string> args)
        {

            var nutchJob = new NutchJob()
            {
                crawlId = crawlId,
                type = jobType,
                confId = confId,
                args = args ?? new Dictionary<string, string>(),
            };
            
            var jsonNutchJob = JsonConvert.SerializeObject(nutchJob);

            var resp = HttpClient.PostJson(string.Format("{0}/job/create/", _nutchConnStr), jsonNutchJob);
           
            var jsonResult = HttpClient.GetResponseStream(resp);

            if (jsonResult != null)
            {
                return JsonConvert.DeserializeObject<NutchResults>(jsonResult);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetPropertyValue(string configName, string propertyName)
        {
            //config/default/solr.server.url
            return HttpClient.GetRequest(string.Format("{0}/config/{1}/{2}", _nutchConnStr, configName, propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string SetPropertyValue(string configName, string propertyName)
        {
            /*
              PUT /config/{configuration name}/{property}
                    Examples:
            PUT /config/default/http.agent.name
              * */

            return null;
        }
       

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            HttpClient.CallWebRequest(string.Format("{0}admin/stop", _nutchConnStr), "POST");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public string GetJobStatus(string jobId)
        {
            var resp = HttpClient.CallWebRequest(string.Format("{0}/job/{1}", _nutchConnStr, jobId));

            var jsonResult =  HttpClient.GetResponseStream(resp);

            if (jsonResult != null)
            {
                var result = JsonConvert.DeserializeObject<NutchResults>(jsonResult);

                if(result != null)
                {
                    return result.state;
                }
            }
            
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="crawlId"></param>
        /// <param name="confId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public NutchResults RunJobWait(string jobType, string crawlId, string confId, Dictionary<string, string> args)
        {
            var results = RunJob(jobType, crawlId, confId, args);

            if (results != null)
            {
                while (results.state == "RUNNING" || results.state == "IDLE")
                {
                    Task.Delay(1000);

                    results.state = GetJobStatus(results.id);
                }
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlDir"></param>
        /// <param name="crawlId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Crawl(string crawlId, string config="default")
        {
            if (string.IsNullOrEmpty(_urlDir))
            {
                throw new Exception("urlDir is not defined.");
            }

            var noargs = new Dictionary<string, string>();
            
            var argsInject = new Dictionary<string, string>();
            argsInject.Add("url_dir", _urlDir);

            var argsGen = new Dictionary<string, string>();

            if(_topN != 0)
                argsGen.Add("topN", _topN.ToString());
            
            var argsParse = new Dictionary<string, string>();
            argsParse.Add("noFilter", "true");

            var argsIndex = new Dictionary<string, string>();
            argsIndex.Add("crawlId", crawlId);

            // TO DO: DEDUP
            var jobWorkflow = new List<string>() { "INJECT", "GENERATE", "FETCH", "PARSE", "UPDATEDB", "INVERTLINKS", "INDEX" };
            var jobArgs = new List<Dictionary<string, string>>() { argsInject, argsGen, noargs, argsParse, noargs, noargs, argsIndex };

            int index = 0;

            foreach(var job in jobWorkflow)
            {
                var result = RunJobWait(job, crawlId, config, jobArgs[index++]);

                if (result.state == "FAILED")
                    return false;
            }

            return true;
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="urls"></param>
        /// <returns></returns>
        public string CreateSeedList(string name, string[] urls)
        {
            var endPoint = string.Format("{0}/seed/create", _nutchConnStr);

            var seedList = new SeedList()
            {
                name = name,
                seedUrls =  urls,
            };

            var jsonSeedList = JsonConvert.SerializeObject(seedList);

            var resp = HttpClient.PostJson(endPoint, jsonSeedList);

            var jsonResult = HttpClient.GetResponseStream(resp);
            
            /*
                POST /seed/create
                {
                "name":"name-of-seedlist", 
                "seedUrls":["http://www.example.com",....]
                }
             * */

            return null;
        }
                
    }
}
