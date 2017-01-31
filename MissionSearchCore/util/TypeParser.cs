using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MissionSearch.Util
{
    public static class TypeParser
    {
        public static long ParseLong(string str, long defaultVal = 0)
        {
            long value;

            return long.TryParse(str, out value) ? value : defaultVal;
        }

        public static int ParseInt(string str, int defaultVal = 0)
        {
            int value;

            return int.TryParse(str, out value) ? value : defaultVal;
        }

        public static decimal ParseDecimal(string str)
        {
            decimal value;

            return decimal.TryParse(str, out value) ? value : 0;
        }


        public static double ParseDouble(string str)
        {
            double value;

            return double.TryParse(str, out value) ? value : 0;
        }

        public static DateTime? ParseDateTime(string str)
        {
            DateTime date;

            if (DateTime.TryParse(str, out date))
            {
                return date;
            }

            return null;
        }

        public static DateTime? ParseDateExact(string str)
        {
            DateTimeOffset date;

            if (DateTimeOffset.TryParse(str, out date))
            {
                return date.UtcDateTime.Date;
            }

            return null;
        }

        public static List<int> ParseCSVIntList(string strs)
        {
            var list = new List<int>();

            if (string.IsNullOrEmpty(strs))
                return list;
            
            var strList = strs.Split(',');

            foreach(var str in strList)
            {
                int val=0;

                if (int.TryParse(str, out val))
                    list.Add(val);
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetPropertyTypeName<T>(string name)
        {
            Type t = typeof(T);
            var properties = t.GetProperties();
            var prop = properties.FirstOrDefault(p => p.Name == name);

            if (prop == null)
                return null;

            return prop.PropertyType.Name;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ParseStringData(object obj)
        {
            if (obj == null)
                return "";

            var props = obj.GetType().GetProperties();

            var strData = new StringBuilder();

            foreach (var prop in props)
            {
                switch (prop.PropertyType.Name)
                {
                    case "XhtmlString":

                        var xhtml = prop.GetValue(obj) as IHtmlString;

                        if (xhtml != null)
                        {
                            var str = xhtml.ToHtmlString();
                            strData.Append(str);
                            strData.Append(" ");
                        }

                        break;

                    case "String":

                        var value = prop.GetValue(obj);

                        if (value != null)
                        {
                            strData.Append(value as String);
                            strData.Append(" ");
                        }
                        break;

                    case "string":

                        var strValue = prop.GetValue(obj);

                        if (strValue != null)
                        {
                            strData.Append(strValue as string);
                            strData.Append(" ");
                        }
                        break;

                }
            }

            return strData.ToString();
        }
        
    }
}
