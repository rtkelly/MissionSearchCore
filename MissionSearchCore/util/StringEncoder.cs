using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MissionSearch.Util
{
    public class StringEncoder
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncodeString(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return str;

                return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            }
            catch
            {
                return "";
            }
        }
                       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DecodeString(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return str;

                return Encoding.UTF8.GetString(Convert.FromBase64String(str));
            }
            catch
            {
                return "";
            }
        }

        public static string TrimExtraSpaces(string str)
        {
            var options = RegexOptions.None;
            var regex = new Regex("[ ]{2,}", options);
            return regex.Replace(str, " ");
        }
    }
}
