using MissionSearch.Attributes;
using MissionSearch.Clients;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MissionSearch.Indexers
{
    public class DefaultAssetIndexer<T> : IndexerBase<T>, IAssetIndexer<T> where T : ISearchDocument
    {
        public delegate bool StatusCallBack(string msg=null);

        ISearchClient<T> SearchClient { get; set; }

        int Threshold; // max file size in bytes

        int SourceId; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchClient"></param>
        public DefaultAssetIndexer(ISearchClient<T> srchClient, int threshold, int sourceId)
        {
            if (srchClient == null)
                throw new NotImplementedException("Search Client not implemented");

            SearchClient = srchClient;
            Threshold = threshold;
            SourceId = sourceId;

            _logger = SearchFactory<T>.Logger;
                        
        }

        public IndexResults RunFullIndex(IEnumerable<ISearchableAsset> assets, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var results = RunUpdate(assets, statusCallback, indexerCallback);

            results.DeleteCnt = PurgeDeletedDocuments(assets);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        /// <returns></returns>
        public IndexResults RunUpdate(IEnumerable<ISearchableAsset> assets, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var results = new IndexResults();
                        
            results.TotalCnt = 0;
            results.ErrorCnt = 0;
           
            foreach (var asset in assets)
            {
                try
                {
                    if (asset.NotSearchable)
                    {
                        if(SearchClient.Search("id:" + asset.SearchId).Results.Any())
                        {
                            SearchClient.Delete("id:" + asset.SearchId);
                            results.DeleteCnt++;
                        }
                        continue;
                    }

                    var doc = CreateSolrDoc(asset, results);

                    if (indexerCallback != null)
                    {
                        doc = indexerCallback(doc, asset);
                    }

                    if (statusCallback != null)
                    {
                        if (!statusCallback())
                        {
                            results.Stopped = true;
                            return results;
                        }
                    }

                    SearchClient.Post(doc);
                    results.TotalCnt++;

                    //if (++cnt == 1000)
                    //{
                    //    SearchClient.PostCommit();
                     //   cnt = 0;
                      //  System.Threading.Thread.Sleep(5000);
                    //}
                }
                catch(Exception ex)
                {
                    LogError(string.Format("Indexing failed for ID: {0} NAME:{1}. {2} {3}", asset.SearchId, asset.Name, ex.Message, ex.StackTrace));
                    results.ErrorCnt++;
                }
            }

            SearchClient.PostCommit();
            SearchClient.Close();
                     
            return results;
        }
                        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public T Update(ISearchableAsset asset)
        {
            var doc = CreateSolrDoc(asset, null);

            SearchClient.Post(doc);
            SearchClient.PostCommit();
            SearchClient.Close();

            return doc;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        public int Delete(IEnumerable<ISearchableAsset> assets)
        {
            foreach (var asset in assets)
            {
                SearchClient.DeleteById(asset.SearchId);
            }

            return assets.Count();
        }

               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public void Delete(ISearchableAsset asset)
        {
            SearchClient.DeleteById(asset.SearchId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private T CreateSolrDoc(ISearchableAsset asset, IndexResults results)
        {
            var doc = (T)Activator.CreateInstance(typeof(T), new object[] { });

            doc.id = asset.SearchId;
            doc.title = asset.Name;
            doc.url = asset.SearchUrl;
            doc.timestamp = asset.Changed;
            doc.sourceid = SourceId;
                       
            var assetProps = asset.GetType().GetProperties();
            var docProps = doc.GetType().GetProperties();
                        
            var pageCrawlProps = asset.CrawlProperties as CrawlerContentSettings;

            // load crawl properties
            if (pageCrawlProps != null && pageCrawlProps.Content != null && pageCrawlProps.Content.Any())
            {
                foreach (var crawlPropContent in pageCrawlProps.Content)
                {
                    var docProp = docProps.FirstOrDefault(p => p.Name == crawlPropContent.Name);

                    if (docProp != null)
                    {
                        SetPropertyValue(doc, docProp, crawlPropContent.Value);
                    }
                }
            }

            var pageBaseTypes = new List<System.Type>();

            var baseType = asset.GetType().BaseType;

            while (baseType != null)
            {
                pageBaseTypes.Add(baseType);
                baseType = baseType.BaseType;
            }

            pageBaseTypes.Reverse();

            foreach (var bType in pageBaseTypes)
            {
                GetBaseProperties(asset, doc, docProps, bType);
            }
                       

            if (!asset.DisableExtract)
            {
                try
                {
                    var blob = asset.AssetBlob;

                    if (blob != null && blob.Length <= Threshold)
                    {
                        doc = SearchClient.Extract(doc, blob);
                    }
                }
                catch(Exception ex)
                {
                    LogWarning(string.Format("Extraction failed for ID: {0} NAME:{1}. {2}", asset.SearchId, asset.Name, ex.Message));
                        
                    if(results != null)
                        results.WarningCnt++;
                }
            }
            

            var docContent = doc.content != null ? HtmlParser.StripHTML(string.Join(" ", doc.content)) : "";

            doc.highlightsummary = (HtmlParser.StripHTML(doc.summary) + " " + docContent + " " + doc.title).Trim();
                        
            return doc;
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        public int PurgeDeletedDocuments(IEnumerable<ISearchableAsset> assets)
        {
            var indexedAssets = SearchClient.Search("sourceid:" + SourceId).Results;

            var notFound = indexedAssets
                                .Where(p => !assets.Any(pg => pg.SearchId == p.id))
                                .ToList();

            foreach (var page in notFound)
            {
                SearchClient.Delete("id:" + page.id);
            }

            SearchClient.PostCommit();

            return notFound.Count();
        }
    }
}
