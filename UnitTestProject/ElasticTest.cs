using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionSearch;
using MissionSearch.Clients;
using MissionSearch.Clients.ElasticSearch;

namespace UnitTestProject
{
    [TestClass]
    public class ElasticTest
    {
        [TestMethod]
        public void TestSearch()
        {
            var client = new ElsClient<SearchDocument>("http://localhost:9200/demoindex");

            var boolRequest = ElsQueryRequest.BoolQuery();

            boolRequest.AddTerm(Els.BoolQuery.must, "content", "test");
            boolRequest.AddTerm(Els.BoolQuery.must, "title", "puppy");
            //boolRequest.AddRange(ElsGlobal.QueryType.filter, "price", 0, 1000);
            //boolRequest.AddDateRange(ElsGlobal.QueryType.filter, "publication_date", DateTime.Now.AddDays(-1), DateTime.Now);

            boolRequest.size =10;
            boolRequest.from = 0;
            
            var resp = client.Search(boolRequest);

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
