﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Clients.ElasticSearch
{
    public interface IElsQuery
    {
        IQuery query { get; set; }
                
       
    }
}
