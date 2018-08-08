﻿using System;
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

            var boolRequest = new BoolQueryRequest();

            //boolRequest.AddMust(new TermQuery("title", "puppy"));
            boolRequest.AddMust(new PrefixQuery("title", "pup"));

            //boolRequest.AddMust(new RangeQuery<DateTime>("publication_date", DateTime.Now.AddDays(-1), DateTime.Now));
            boolRequest.AddMust(new ElsRangeQuery<DateTime>("publication_date",
                DateTime.Now.AddDays(-1), DateFilterQuery.ConditionalTypes.GreaterThenEqual));
                

            //boolRequest.AddMust(new RangeQuery<long>("price", 0, 1000));
                        
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
            
            //client.Post(srchDoc);

        } 
    }
}
