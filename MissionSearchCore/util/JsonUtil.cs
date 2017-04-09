using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public static class JsonUtil
    {
        public static string ToJson(object obj)
        {

            return JsonConvert.SerializeObject(obj);
        }


        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });
        }

        public static string ReadJsonFile(string filePath)
        {
            JObject o1 = JObject.Parse(File.ReadAllText(filePath));
            return o1.ToString();
        }
    }
}
