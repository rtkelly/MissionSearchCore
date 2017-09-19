using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionSearch;
using MissionSearch.Clients;

namespace UnitTestProject
{
    [TestClass]
    public class ElasticTest
    {
        [TestMethod]
        public void TestSearch()
        {
            var client = new ElsClient("http://localhost:9200/demoindex");

            var req = new SearchRequest();

            req.QueryText = "robert";

            var resp = client.Search(req);

        }
        [TestMethod]
        public void TestPost()
        {
            var client = new ElsClient("http://localhost:9200/demoindex");

            client.DefaultType = "cmsdoc";

            var json = " {\"_id\" : \"3\", \"name\" : \"patrick kelly\" }";

            //client.Post(json);

        }

        [TestMethod]
        public void TestPost2()
        {
            var client = new ElsClient<SearchDocument>("http://localhost:9200/demoindex");

            client.DefaultType = "cmsdoc";
                        
            var srchDoc = new SearchDocument()
            {
                id = "4",
                title = "patrick thomas",
            };
            
            client.Post(srchDoc);

        }
    }
}
