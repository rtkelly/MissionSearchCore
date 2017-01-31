using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionSearch.Clients;

namespace UnitTestProject
{
    [TestClass]
    public class SolrClientUnitTests
    {
        [TestMethod]
        public void TestClientTimeout()
        {
            var client = new SolrClient<SearchDocument>("http://192.168.120.215:8983/solr/wef_shard1_replica1");

            client.Timeout = 1000;

            var resp = client.Search("test");
            
        }
    }
}
