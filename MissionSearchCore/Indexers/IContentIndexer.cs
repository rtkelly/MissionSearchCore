using MissionSearch.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Indexers
{
    public interface IContentIndexer<T> where T : ISearchDocument
    {
        new ISearchClient<T> SearchClient { get; set; }

        new IndexResults RunFullIndex(IEnumerable<ContentCrawlProxy> contentItems);

        new IndexResults RunFullIndex(IEnumerable<ContentCrawlProxy> contentItems, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);

        new IndexResults RunUpdate(IEnumerable<ISearchableContent> pages, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);

        new IndexResults RunUpdate(IEnumerable<ContentCrawlProxy> pages, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);

        T Update(ContentCrawlProxy contentItem);

        int Delete(IEnumerable<ISearchableContent> contentItems);

        void Delete(ISearchableContent contentItem);

    }

    public interface IContentIndexer
    {
        ISearchClient SearchClient { get; set; }

        int Delete(List<ISearchableContent> contentItems);

        void Delete(ISearchableContent contentItem);

        IndexResults RunFullIndex(List<ContentCrawlProxy> contentItems);

        IndexResults RunUpdate(List<ContentCrawlProxy> contentItems);
        
        
    }
}
