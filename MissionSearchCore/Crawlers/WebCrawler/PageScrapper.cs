using HtmlAgilityPack;
using MissionSearch.Attributes;
using MissionSearch.Crawlers;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MissionSearch.Util
{
    public class PageScrapper
    {
        
        private ILogger _logger { get; set; }
        
        public PageScrapper()
        {
            _logger = SearchFactory.Logger;
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

                var doc = new HtmlDocument();
                doc.LoadHtml(resp);

                doc.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "footer" || n.Name == "nav")
                    .ToList()
                    .ForEach(n => n.Remove());
                
                var pageContent = HtmlParser.StripHTML(doc.DocumentNode.OuterHtml);

                return StringEncoder.TrimExtraSpaces(pageContent);
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public PageExtractResults ScrapPage(PageExtractRequest req)
        {
            try
            {
                var results = new PageExtractResults();
                var resp = HttpClient.GetResponseStream(HttpClient.CallWebRequest(req.PageUrl));

                var doc = new HtmlDocument();
                doc.LoadHtml(resp);

                req.PageModel.Name = !string.IsNullOrEmpty((req.TitlePattern)) ? HtmlParser.ParseStringFromHtml(doc, req.TitlePattern) : ParseTitleFromHtml(doc, req.PageUrl);

                if (!string.IsNullOrEmpty((req.SummaryPattern)))
                {
                    results.Description = HtmlParser.ParseStringFromHtml(doc, req.SummaryPattern);
                }

                LoadPageProperties(doc, req.PageModel);

                doc.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "footer" || n.Name == "nav")
                    .ToList()
                    .ForEach(n => n.Remove());

                req.PageModel.Content = new List<string>();

                if (req.ContentPattern.Any())
                {
                    var str = new StringBuilder();

                    foreach(var pattern in req.ContentPattern)
                    {
                        req.PageModel.Content.Add(HtmlParser.ParseStringFromHtml(doc, pattern));
                    }
                }
                else
                {
                    req.PageModel.Content.Add(HtmlParser.ParseStringFromHtml(doc, "\\body"));
                }

                return results;
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <param name="page"></param>
        private void LoadPageProperties(HtmlDocument htmlDoc, IWebCrawlPage page)
        {
            var pageProps = page.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            
            foreach (var pageProp in pageProps)
            {
                ProcessHtmlNode(pageProp, htmlDoc, page);
                ProcessHtmlAttributes(pageProp, htmlDoc, page);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageProp"></param>
        /// <param name="htmlDoc"></param>
        /// <param name="page"></param>
        private void ProcessHtmlNode(PropertyInfo pageProp, HtmlDocument htmlDoc, IWebCrawlPage page)
        {
            var customAttr = Attribute.GetCustomAttributes(pageProp, typeof(MapHtmlNode));

            if (!customAttr.Any())
                return;

            var crawlMap = customAttr.First() as MapHtmlNode;

            if (crawlMap == null)
                return;

            switch (pageProp.PropertyType.Name)
            {
                case "List`1":

                    var list = pageProp.GetValue(page) as List<string> ?? new List<string>();

                    foreach (var attr in customAttr.Select(ca => ca as MapHtmlNode))
                    {
                        var value0 = HtmlParser.ParseStringFromHtml(htmlDoc, attr.XPath);
                        list.Add(value0);
                    }

                    break;

                case "DateTime":

                    var value1 = HtmlParser.ParseDateFromHtml(htmlDoc, crawlMap.XPath);

                    if (value1 != null) pageProp.SetValue(page, value1);
                    break;
                
                default:
                    
                    var value2 = HtmlParser.ParseStringFromHtml(htmlDoc, crawlMap.XPath);

                    if (value2 != null) pageProp.SetValue(page, value2);
                    break;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageProp"></param>
        /// <param name="htmlDoc"></param>
        /// <param name="page"></param>
        private void ProcessHtmlAttributes(PropertyInfo pageProp, HtmlDocument htmlDoc, IWebCrawlPage page)
        {
            var customAttr = Attribute.GetCustomAttributes(pageProp, typeof(MapAttribute));

            if (!customAttr.Any())
                return;

            var crawlMap = customAttr.First() as MapAttribute;

            if (crawlMap == null)
                return;

            switch (pageProp.PropertyType.Name)
            {
                case "List`1":
                    {
                        var list = pageProp.GetValue(page) as List<string> ?? new List<string>();

                        var result = HtmlParser.ParseAtrtributeFromNode(htmlDoc, crawlMap.XPath, crawlMap.AttributeName);

                        if (!string.IsNullOrEmpty(result))
                        {
                            list.Add(result);
                            pageProp.SetValue(page, list);
                        }

                        break;
                    }
                case "DateTime":
                    {
                        var result = HtmlParser.ParseAtrtributeFromNode(htmlDoc, crawlMap.XPath, crawlMap.AttributeName);

                        if (!string.IsNullOrEmpty(result))
                        {
                            var date = TypeParser.ParseDateTime(result);

                            if (date != null)
                                pageProp.SetValue(page, date.Value);
                        }
                        break;
                    }
                default:
                    {
                        var result = HtmlParser.ParseAtrtributeFromNode(htmlDoc, crawlMap.XPath, crawlMap.AttributeName);
                        if (!string.IsNullOrEmpty(result)) pageProp.SetValue(page, result);
                        break;
                    }

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
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
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
                    return HtmlParser.StripHTML(HttpUtility.HtmlDecode(metaTitle));

                var headTitle = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;

                if (!string.IsNullOrEmpty(headTitle))
                    return HtmlParser.StripHTML(HttpUtility.HtmlDecode(headTitle));
            }
            catch
            {
                //ignore
            }

            return url;
        }
    }
}
