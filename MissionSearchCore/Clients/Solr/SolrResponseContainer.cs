using System.Collections.Generic;

namespace MissionSearch.Clients
{

    internal class SolrResponseContainer
    {
        public SolrResponseHeader responseHeader { get; set; }
        public SolrResponse response { get; set; }
        public SolrFacetCounts facet_counts { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> highlighting { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SolrResponseContainer<T>
    {
        public SolrResponseHeader responseHeader { get; set; }
        public SolrResponse<T> response { get; set; }
        public SolrFacetCounts facet_counts { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> highlighting { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class SolrResponseHeader
    {
        public int status { get; set; }
        public int QTime { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SolrResponse<T>
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public List<T> docs { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SolrResponse
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public List<dynamic> docs { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class SolrFacetCounts
    {
        public Dictionary<string, List<string>> facet_fields { get; set; }
        public Dictionary<string, SolrFacetRanges> facet_ranges { get; set; }
        public Dictionary<string, int> facet_queries { get; set; }
    }

    internal class SolrFacetRanges
    {
        public List<string> counts { get; set; }
        public string gap { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public int before { get; set; }
        public int after { get; set; }
        public int between { get; set; }
    }

    public class SolrTermsContainer
    {
        public Dictionary<string, List<string>> terms { get; set; }
    }

    
    
}
