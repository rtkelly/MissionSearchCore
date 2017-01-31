using HtmlAgilityPack;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MissionSearch.Util
{
    public class PageScrapper
    {
        private string ContainterPath =  "//div[@id='container']";

        public PageScrapper()
        {
            
        }
         
        public PageScrapper(string containerPath)
        {
            ContainterPath = containerPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ScrapPage(string url)
        {
            try
            {
                var resp = HttpClient.GetResponseStream(HttpClient.CallWebRequest(url));

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(resp);

                doc.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "footer" || n.Name == "nav")
                    .ToList()
                    .ForEach(n => n.Remove());

                var containerNode = (string.IsNullOrEmpty(ContainterPath)) ? null : doc.DocumentNode.SelectSingleNode(ContainterPath);

                var pageContent = (containerNode == null) ? HtmlParser.StripHTML(doc.DocumentNode.OuterHtml) : HtmlParser.StripHTML(containerNode.InnerHtml);

                return StringEncoder.TrimExtraSpaces(pageContent);
            }
            catch
            {
                return string.Empty;
            }
        }

        public PageScrapperResults ScrapPage2(string url)
        {
            try
            {
                var results = new PageScrapperResults();
                var resp = HttpClient.GetResponseStream(HttpClient.CallWebRequest(url));

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(resp);
                                
                results.Title = HtmlParser.StripHTML(HttpUtility.HtmlDecode(ParseTitleFromHtml(doc, url)));
                //results.Description = ParseDescriptionFromHtml(doc, url);
                
                doc.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "footer" || n.Name == "nav")
                    .ToList()
                    .ForEach(n => n.Remove());

                var containerNode = (string.IsNullOrEmpty(ContainterPath)) ? null : doc.DocumentNode.SelectSingleNode(ContainterPath);

                results.Content = (containerNode == null) ? HtmlParser.StripHTML(doc.DocumentNode.OuterHtml) : HtmlParser.StripHTML(containerNode.InnerHtml);

                return results;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ParseDescriptionFromHtml(HtmlDocument doc, string url)
        {
            try
            {
                var metaDescription1 = doc.DocumentNode.SelectSingleNode("//meta/@og:description").InnerText;

                if (!string.IsNullOrEmpty(metaDescription1))
                    return metaDescription1;
            }
            catch
            {
                // ignore
            }

            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ParseTitleFromHtml(HtmlDocument doc, string url)
        {
            try
            {
                var h1Title = doc.DocumentNode.SelectSingleNode("//body//h1").InnerText;

                if (!string.IsNullOrEmpty(h1Title))
                    return h1Title;

                var metaTitle = doc.DocumentNode.SelectSingleNode("//meta/@title").InnerText;

                if (!string.IsNullOrEmpty(metaTitle))
                    return metaTitle;

                var headTitle = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;

                if (!string.IsNullOrEmpty(headTitle))
                    return headTitle;
            }
            catch
            {
                //ignore
            }

            return url;
        }
    }
}
