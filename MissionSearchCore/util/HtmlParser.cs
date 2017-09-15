using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MissionSearch.Util
{
    public class HtmlParser
    {
        HtmlDocument _htmlDocument;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        public HtmlParser(string html)
        {
            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(html);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public string ParseInnerHtml(string xpath)
        {
            var node = _htmlDocument.DocumentNode.SelectSingleNode(xpath);

            return node == null ? "" : node.InnerHtml;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public string ParseStripInnerHtml(string xpath)
        {
            var node = _htmlDocument.DocumentNode.SelectSingleNode(xpath);

            return node == null ? "" : Tokenize(StripHTML(node.InnerHtml));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHTML(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                    
            return StripComments(StripFormating(Regex.Replace(input, "<.*?>", String.Empty)));
        }

        public static string StripComments(string input)
        {
            return input.Replace("<!--", "").Replace("-->", "").Trim();
        }

        public static string StripFormating(string input)
        {
            return Regex.Replace(input, "\n|\r|\t", " ").Trim();
        }

        public static string Tokenize(string input)
        {
           var tokens = input.Split(' ');

           return string.Join(" ", tokens.Where(t => !string.IsNullOrEmpty(t)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public HtmlNodeCollection GetNodes(string xpath)
        {
            var collection = _htmlDocument.DocumentNode.SelectNodes(xpath);
            return collection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xpath"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string ParseAtrtributeFromNode(HtmlDocument doc, string xpath, string attributeName)
        {
            var nodes = doc.DocumentNode.SelectNodes(xpath);

            foreach (var node in nodes)
            {
                string content = node.GetAttributeValue(attributeName, "");

                if (!string.IsNullOrEmpty(content))
                    return content;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static string ParseStringFromHtml(HtmlDocument doc, string xpath)
        {
            var node = doc.DocumentNode.SelectSingleNode(xpath);

            if (node == null)
                return null;

            var value = node.InnerText;

            if (string.IsNullOrEmpty(value))
                return null;

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static DateTime? ParseDateFromHtml(HtmlDocument doc, string xpath)
        {

            var value = doc.DocumentNode.SelectSingleNode(xpath).InnerText;

            if (!string.IsNullOrEmpty(value))
            {
                return TypeParser.ParseDateTime(value);
            }

            return null;
        }

      

    }
}
