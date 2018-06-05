using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public interface IElsQueryRequest
    {
        int from { get; set; }

        int size { get; set; }

        //ElsQuery query { get; set; }

        string GetJsonQuery();
    }
}
