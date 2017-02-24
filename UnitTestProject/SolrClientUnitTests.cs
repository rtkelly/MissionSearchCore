using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionSearch.Clients;
using MissionSearch;
using MissionSearch.Search.Query;
using MissionSearch.Search.Facets;
using System.Linq;
using System.Dynamic;

namespace UnitTestProject
{
    [TestClass]
    public class SolrClientUnitTests
    {
        string SolrConnectionString = "http://192.168.120.215:8983/solr/wef_shard1_replica1";

        [TestMethod]
        public void TestClientTimeout()
        {
            var client = new SolrClient<SearchDocument>(SolrConnectionString);

            client.Timeout = 1000;

            var resp = client.Search("test");
            
        }


        [TestMethod]
        public void TestPostDocument()
        {
            var client = new SolrClient(SolrConnectionString);

            var json = "{'id':'1','title':'some data'}";
                        
            client.Post(json);

            var resp = client.Search("\"some data\"");

            if (resp.Results.Any())
            {
                var title = resp.Results.First().title;
            }
            
        }
    }
}
