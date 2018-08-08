using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public class ElasticResponseContainer
    {
        public int took { get; set; }

    }
    public class ElasticResponseContainer<T>
    {
        
        public Hits<T> hits { get; set;  }
        public Dictionary<string, List<Bucket>> aggregations { get; set; }
        
    }

    public class Hits<T>
    {
        public int total { get; set; }

        public List<Hit<T>> hits { get; set; }
    }

    public class Hit<T>
    {
        public int total { get; set; }

        public T _source { get; set; }
    }
        
    public class Bucket
    {
        public string key { get; set; }

        public int doc_count { get; set; }

    }
}
