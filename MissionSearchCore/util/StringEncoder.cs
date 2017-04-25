using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
                
                //return Zip(str);
                //return WebUtility.UrlEncode(str);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            }
            catch
            {
                return "";
            }
        }
        
        public static string UrlEncodeString(string str)
        {
            return WebUtility.UrlEncode(str);
        }

        public static string UrlDecodeString(string str)
        {
            return WebUtility.UrlDecode(str);
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

                //return Unzip(str);
                //return WebUtility.UrlDecode(str);
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
                

        public static string Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static string Unzip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

    }
}
