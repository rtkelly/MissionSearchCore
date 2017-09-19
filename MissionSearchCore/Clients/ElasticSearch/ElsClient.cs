using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MissionSearch.Clients.ElasticSearch;
using System.IO;

namespace MissionSearch.Clients
{
    public class ElsClient : ISearchClient
    {
        string _srchConnStr;
        public string SrchConnStr { get { return _srchConnStr; } }
        public int Timeout { get; set; }
        public string DefaultType { get; set; }

        public ElsClient(string srchConnectionString)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Elasticsearch server is undefined");

            _srchConnStr = srchConnectionString;

            DefaultType = "cmsdoc";
        }

        public void Close()
        {

        }

        public void Commit()
        {

        }

        public void Delete(string query)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public string FileExtract(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }

        public List<dynamic> GetAll(string queryText)
        {
            throw new NotImplementedException();
        }

        public List<string> GetTerms(string fieldName, string term)
        {
            throw new NotImplementedException();
        }

        public void Post(string jsonDoc)
        {
            if (jsonDoc == null)
                return;

            var idMatch = "\"_id\":\"(.+?)\"";

            var match = Regex.Match(jsonDoc.Replace(" ", ""), idMatch);

            if (!match.Success)
                return;

            var id = match.Groups[1];

            var doc = Regex.Replace(jsonDoc, idMatch + ",", "");

            var endPoint = string.Format("{0}/{1}/{2}", _srchConnStr, DefaultType, id);

            var bytes = Encoding.UTF8.GetBytes(string.Format("{0}", doc));
            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = "PUT";
            request.ContentType = "application/json";

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                var resp = (HttpWebResponse)request.GetResponse();

                resp.Close();
            }

        }

        public void Reload()
        {

        }

        public SearchResponse Search(SearchRequest request)
        {
            var endPoint = string.Format("{0}/_search", _srchConnStr);



            var resp = new SearchResponse();

            return resp;
        }
    }

    public class ElsClient<T> : ElsClient, ISearchClient<T> where T : ISearchDocument
    {
        public ElsClient(string srchConnectionString) : base(srchConnectionString)
        {
        }

        public void Post(T doc)
        {
            var settings = new JsonSerializerSettings();
            //settings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

            var jsonDoc = JsonConvert.SerializeObject(doc, settings);

            jsonDoc = jsonDoc.Replace("\"id\"", "\"_id\"");

            Post(jsonDoc);
        }

        public SearchResponse<T> Search(string queryText)
        {
            throw new NotImplementedException();
        }

        List<T> ISearchClient<T>.GetAll(string queryText)
        {
            /*
             * POST demoindex/_search
                    {
                      "query": { "match_all": {} }
                    }

             */
            var list = new List<T>();

            return list;
        }

        SearchResponse<T> ISearchClient<T>.Search(SearchRequest request)
        {
            var endPoint = string.Format("{0}/_search", SrchConnStr);
                        
            var srchResponse = new SearchResponse<T>();
                                    
            var jsonDoc = JsonConvert.SerializeObject(ElsQueryBuilder.BuildSearchQuery(request), new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            });

            jsonDoc = jsonDoc.Replace("query_bool", "bool");

            var bytes = Encoding.UTF8.GetBytes(string.Format("{0}", jsonDoc));
            var httpRequest = (HttpWebRequest)WebRequest.Create(endPoint);

            if (Timeout > 0) httpRequest.Timeout = Timeout;

            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";

            using (var requestStream = httpRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                
                using (var webResponse = (HttpWebResponse)httpRequest.GetResponse())
                {
                    using (var webStream = webResponse.GetResponseStream())
                    {
                        if (webStream == null)
                            return srchResponse;

                        using (var rdr = new StreamReader(webStream))
                        {
                            srchResponse.JsonResponse = rdr.ReadToEnd();
                        }
                    }
                }
            }

           var responseContainer = JsonConvert.DeserializeObject<ElasticResponseContainer<T>>(srchResponse.JsonResponse, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            });

            srchResponse.TotalFound = responseContainer.hits.total;
            srchResponse.Results = responseContainer.hits.hits.Select(h => h._source).ToList();

            srchResponse.PageSize = request.PageSize;
            srchResponse.CurrentPage = request.CurrentPage;
            srchResponse.Success = true;
            
            return srchResponse;

        }
    }
}
