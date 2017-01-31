using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Indexers
{
    public interface IAssetIndexer<T> where T : ISearchDocument 
    {
        IndexResults RunFullIndex(IEnumerable<ISearchableAsset> assets, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);
        IndexResults RunUpdate(IEnumerable<ISearchableAsset> assets, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback);
        T Update(ISearchableAsset asset);
        
        int Delete(IEnumerable<ISearchableAsset> assets);
        void Delete(ISearchableAsset asset);
        //void PurgeDeletedDocuments(IEnumerable<ISearchableAsset> assets);
    }
    
}
