﻿using MissionSearch.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Indexers
{
    
    public interface IContentIndexer<T>  where T : ISearchDocument 
    {
        ISearchClient<T> SearchClient { get; set; }

        IndexResults RunFullIndex(IEnumerable<ISearchableContent> pages, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);
        IndexResults RunUpdate(IEnumerable<ISearchableContent> pages, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);
        T Update(ISearchableContent page);

        int Delete(IEnumerable<ISearchableContent> pages);
        void Delete(ISearchableContent page);
        //void PurgeDeletedDocuments(IEnumerable<ISearchableContent> pages);
    }
}
