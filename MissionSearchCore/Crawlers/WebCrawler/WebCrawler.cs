using HtmlAgilityPack;
using MissionSearch.Indexers;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MissionSearch.Crawlers
{
    public class WebCrawler<C, T> : ICrawler
        where T : ISearchDocument
        where C : IWebCrawlPage
    {
        public WebCrawlJob _crawlSettings { get; set; }

        private IContentIndexer<T> _Indexer;
        
        private Util.ILogger _logger { get; set; }

        private List<string> LinksProcessed { get; set; }
        
        private List<string> LinksToIndex { get; set; }

        private PageScrapper _pageScrapper { get; set; }

        private string BaseUrl { get; set; }
        
        private string BaseSchema { get; set; }

        private Global<T>.StatusCallBack _statusCallback { get; set; }

        public WebCrawler(WebCrawlJob crawlSettings, Global<T>.StatusCallBack statusCallback=null)
        {
            _crawlSettings = crawlSettings;
            _logger = SearchFactory.Logger;
            _Indexer = SearchFactory<T>.ContentIndexer;
            _pageScrapper = new PageScrapper();
            _statusCallback = statusCallback;
        }

        public WebCrawler(WebCrawlJob crawlSettings, IContentIndexer<T> indexer, Global<T>.StatusCallBack statusCallback = null)
        {
            _crawlSettings = crawlSettings;
            _logger = SearchFactory.Logger;
            _Indexer = indexer;
            _pageScrapper = new PageScrapper();
            _statusCallback = statusCallback;

        }
                
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool HandleStatusCallBack()
        {
            if (_statusCallback != null)
            {
                if(!_statusCallback())
                {
                    LoggerInfo("Crawler has been stopped");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CrawlerResults Run()
        {
            var results = new CrawlerResults
            {
                SourceId = _crawlSettings.SourceId
            };
            
            var startTime = DateTime.Now;

            LoggerInfo("Starting Web Crawl");

            BaseUrl = UrlParser.GetHostName(_crawlSettings.SeedUrl);
            BaseSchema = UrlParser.GetSchema(_crawlSettings.SeedUrl);
                        
            var seedPageResp =  HttpClient.GetRequest(_crawlSettings.SeedUrl);

            LoggerInfo(string.Format("Crawling {0}", _crawlSettings.SeedUrl));

            LinksProcessed = new List<string>();
            LinksToIndex = new List<string>();

            var links = GetLinks(seedPageResp);
            var depth = 1;
            
            ProcessLinks(links, depth);

            var searchableContent = new List<IWebCrawlPage>();
                        
            foreach(var link in LinksToIndex)
            {
                LoggerDebug(string.Format("Extracting {0}", link));

                var page = ProcessPage(link);

                if(page != null)
                    searchableContent.Add(page);

                if (HandleStatusCallBack())
                    return results;

                System.Threading.Thread.Sleep(1000);
            }

            LoggerInfo("Running Indexer");
                        
                        
            var indexResults = _Indexer.RunUpdate(searchableContent, null, null);

            results.TotalCnt = indexResults.TotalCnt;
            results.ErrorCnt = indexResults.ErrorCnt;
            results.Duration = (DateTime.Now - startTime);

            LoggerInfo("Web Crawler finished.");

            return results;
        }


       /// <summary>
       /// 
       /// </summary>
       /// <param name="links"></param>
       /// <param name="depth"></param>
        private void ProcessLinks(List<string> links, int depth)
        {
            
            foreach(var link in links)
            {
                ProcessLink(link, depth);
                System.Threading.Thread.Sleep(1000);

                if (HandleStatusCallBack())
                    return;
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private C ProcessPage(string link)
        {
            var searchable = (C)Activator.CreateInstance(typeof(C), new object[] { });

            searchable._ContentID = UrlParser.GetHostandPath(link);
            searchable.SearchUrl = link;
            searchable.Hostname = BaseUrl;

            var req = new PageExtractRequest()
            {
                PageUrl =  link,
                PageModel = searchable,
            };
            
            var results = _pageScrapper.ScrapPage(req);

            if (results == null)
                return default(C);
                        
            return searchable;
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="link"></param>
        /// <param name="depth"></param>
        private void ProcessLink(string link, int depth)
        {
            try
            {
                if (ContainsIndexUrlPattern(link) && !ContainsIndexSkipPattern(link))
                {
                    LinksToIndex.Add(link);
                }

                if (ContainsCrawlUrlPattern(link) && !ContainsCrawlSkipPattern(link))
                {
                    var newDepth = depth + 1;

                    if (newDepth > _crawlSettings.Depth)
                        return;

                    LoggerDebug(string.Format("Crawling {0} depth {1}", link, depth));

                    var resp = HttpClient.GetRequest(link);

                    var links = GetLinks(resp);

                    ProcessLinks(links, newDepth);
                }
            }
            catch(Exception ex)
            {
                LoggerError(string.Format("Error Crawling {0} {1}", link, ex.Message));
                
                LoggerDebug(ex.StackTrace);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public List<string> GetLinks(string html)
        {
            var links = new List<string>();

            var doc = new HtmlDocument();

            try
            {
                doc.LoadHtml(html);

                var nodes = doc.DocumentNode.SelectNodes("//a[@href]");

                if (nodes == null)
                    return links;

                foreach (HtmlNode link in nodes)
                {
                    var href = link.GetAttributeValue("href", string.Empty);

                    var linkUrl = (!href.StartsWith("http")) ?
                            string.Format("{0}://{1}{2}", BaseSchema, BaseUrl, href) : href;
                        
                    if (_crawlSettings.LinkCleanupPattern != null)
                    {
                        foreach (var cleanupPattern in _crawlSettings.LinkCleanupPattern)
                        {
                            linkUrl = Regex.Replace(linkUrl, cleanupPattern, "");
                        }
                    }

                    if (!LinksProcessed.Contains(linkUrl))
                    {
                        LinksProcessed.Add(linkUrl);
                        links.Add(linkUrl);
                    }
                }
            }
            catch(Exception ex)
            {
                LoggerError(string.Format("Error Crawling {0}", ex.Message));
                LoggerDebug(ex.StackTrace);
            }
                       
            return links;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ContainsCrawlUrlPattern(string url)
        {
            if (_crawlSettings.CrawlUrlPattern != null)
            {
                foreach (var pattern in _crawlSettings.CrawlUrlPattern)
                {
                    if (Regex.IsMatch(url, pattern.Trim()))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ContainsIndexUrlPattern(string url)
        {
            if (_crawlSettings.IndexUrlPattern != null)
            {
                foreach (var pattern in _crawlSettings.IndexUrlPattern)
                {
                    if (Regex.IsMatch(url, pattern.Trim()))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ContainsIndexSkipPattern(string url)
        {
            if (_crawlSettings.IndexSkipUrlPattern != null)
            {
                foreach (var skipPattern in _crawlSettings.IndexSkipUrlPattern)
                {
                    if (Regex.IsMatch(url, skipPattern.Trim()))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ContainsCrawlSkipPattern(string url)
        {
            if (_crawlSettings.CrawlSkipUrlPattern != null)
            {
                foreach (var skipPattern in _crawlSettings.CrawlSkipUrlPattern)
                {
                    if(Regex.IsMatch(url, skipPattern.Trim()))
                        return true;
                }
            }

            return false;
        }

        private void LoggerDebug(string msg)
        {
            if (_logger != null)
                _logger.Debug(msg);
        }
                      
        private void LoggerInfo(string msg)
        {
            if (_logger != null)
                _logger.Info(msg);

        }

        private void LoggerError(string msg)
        {
            if (_logger != null)
                _logger.Error(msg);

        }
    }
}
