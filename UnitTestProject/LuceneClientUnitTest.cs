using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionSearch.Clients;
using MissionSearch;
using System.Linq;
using MissionSearch.LuceneClient;

namespace UnitTestProject
{
    [TestClass]
    public class LuceneClientUnitTest
    {
        string IndexPath = @"c:\testindex";

        
        [TestMethod]
        public void TestPostDocument()
        {
            var client = new LuceneClient<SearchDocument>(IndexPath);

            var srchDoc = new SearchDocument()
            {
                id = "testid2",
                title = "test title 2",
                summary = "test summary 2",
                url = "test url 2",
            };

            client.Post(srchDoc);
            client.PostCommit();   
        }

        [TestMethod]
        public void TestSearch()
        {
            var client = new LuceneClient(IndexPath);
            
            var srchRequest = new SearchRequest()
            {
                QueryText = "test",
            };
            
            var resp = client.Search(srchRequest);

            if (resp.Results.Any())
            {
                var title = resp.Results.First().title;
            }
        }

        [TestMethod]
        public void TestSearch2()
        {
            var client = new LuceneClient(@"C:\iisweb\asrm\ASRM\App_Data\Index\Main");

            client.SearchDefaultField = "EPISERVER_SEARCH_TITLE";

            var srchRequest = new SearchRequest()
            {
                QueryText = "*:*",
            };

            srchRequest.QueryOptions.Add(new FilterQuery("EPISERVER_SEARCH_TYPE", FilterQuery.ConditionalTypes.Contains, "ASRM.Models.Pages.BasePageData"));

            var resp = client.Search(srchRequest);

            if (resp.Results.Any())
            {
                var row = resp.Results.First();

                var title = row.EPISERVER_SEARCH_TITLE;
            }
        }
         
    }
    
}
