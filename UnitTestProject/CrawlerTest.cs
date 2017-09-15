using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionSearch.Crawlers;
using MissionSearch.Util;
using System.Net;

namespace MissionSearchTests
{
    /// <summary>
    /// Summary description for CrawlerTest
    /// </summary>
    [TestClass]
    public class CrawlerTest
    {
        [TestMethod]
        public void TestWebCrawl()
        {
          
            var req = new WebCrawlJob()
            {
                SeedUrl = "http://stormwater.wef.org/2016/12/",
                Depth = 1,
                SourceId = 103,
                CrawlUrlPattern = new List<string>()
                {
                    //"http://stormwater.wef.org/\\d.*/\\d.*/"
                },
                IndexUrlPattern = new List<string>()
                {
                      "http://stormwater.wef.org/\\d.*/\\d.*/.*",
                },
                TitlePattern = new List<string>() { "\\body\\h1" },
                SummaryPattern = "/meta[@property='og:description']",
                ContentPattern = new List<string>()
                {
                    "/div[@id='content']"
                },
               
            };
          
            var crawler = new WebCrawler<WebCrawlBasePage, WebCrawlerSearchDoc>(req);

            //var results = crawler.Run();
            
        }
    }
}
