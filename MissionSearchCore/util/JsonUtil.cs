using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
