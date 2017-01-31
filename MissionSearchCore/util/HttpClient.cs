using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public class HttpClient
    {
        public static HttpWebResponse CallWebRequest(HttpClientRequest requestSettings)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestSettings.EndPoint);

            request.Method = requestSettings.Method;
            request.Referer = requestSettings.Referrer;
            request.Timeout = requestSettings.Timeout;
            //request.Host = = requestSettings.Host;
            
            var resp = (HttpWebResponse)request.GetResponse();

            return resp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static HttpWebResponse CallWebRequest(string endPoint, string method="GET")
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);

            request.Method = method;
            
            var resp = (HttpWebResponse)request.GetResponse();
            
            return resp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public static string GetRequest(string endPoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);

            var webResponse = (HttpWebResponse)request.GetResponse();

            var str = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="jsonDoc"></param>
        /// <returns></returns>
        public static HttpWebResponse PostJson(string endPoint, string jsonDoc)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);

            request.Method = "POST";
            request.ContentType = "application/json";

            var bytes = Encoding.UTF8.GetBytes(jsonDoc);

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                var resp = (HttpWebResponse)request.GetResponse();
                                
                return resp;
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static string GetResponseStream(HttpWebResponse resp)
        {
            var webStream = resp.GetResponseStream();

            if (webStream != null)
            {
                using (var rdr = new StreamReader(webStream))
                {
                    return rdr.ReadToEnd();
                }
            }

            return null;
        }

        
    }
}
