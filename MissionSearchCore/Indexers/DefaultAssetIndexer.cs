using MissionSearch.Clients;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MissionSearch.Indexers
{
    public class DefaultAssetIndexer<T> : IndexerBase<T>, IAssetIndexer<T> where T : ISearchDocument
    {
        public delegate bool StatusCallBack(string msg=null);

        ISearchClient<T> SearchClient { get; set; }

        int Threshold; // max file size in bytes
        
        public DefaultAssetIndexer(int threshold, int sourceId)
        {
            SearchClient = SearchFactory<T>.SearchClient;
            _logger = SearchFactory.Logger;
            Threshold = threshold;
            _sourceId = sourceId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchClient"></param>
        /// <param name="threshold"></param>
        /// <param name="sourceId"></param>
        public DefaultAssetIndexer(ISearchClient<T> srchClient, int threshold, int sourceId)
        {
            if (srchClient == null)
                throw new NotImplementedException("Search Client not implemented");

            SearchClient = srchClient;
            Threshold = threshold;
            _sourceId = sourceId;

            _logger = SearchFactory.Logger;
                        
        }

        public IndexResults RunFullIndex(IEnumerable<ContentCrawlProxy> assets, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var items = assets as IList<ContentCrawlProxy> ?? assets.ToList();

            var results = RunUpdate(items, statusCallback, indexerCallback);

            results.DeleteCnt = PurgeDeletedDocuments(items.Select(p => p.ContentItem as ISearchableAsset).ToList());

            return results;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        /// <returns></returns>
        public IndexResults RunUpdate(IEnumerable<ContentCrawlProxy> parameters, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var results = new IndexResults();
                        
            results.TotalCnt = 0;
            results.ErrorCnt = 0;
           
            foreach (var asset in parameters)
            {
                try
                {
                    /*
                    if (asset.NotSearchable)
                    {
                        if(SearchClient.Search("id:" + asset._ContentID).Results.Any())
                        {
                            SearchClient.Delete("id:" + asset._ContentID);
                            results.DeleteCnt++;
                        }
                        continue;
                    }
                     * */

                    var doc = CreateSearchDoc(asset, results);

                    if (indexerCallback != null)
                    {
                        doc = indexerCallback(doc, asset.ContentItem);
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
                    LogError(string.Format("Indexing failed for ID: {0} NAME:{1}. {2} {3}", asset.ContentItem._ContentID, asset.ContentItem.Name, ex.Message, ex.StackTrace));
                    results.ErrorCnt++;
                }
            }

            SearchClient.Commit();
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
            var parameters = new ContentCrawlProxy() { ContentItem = asset };
            var doc = CreateSearchDoc(parameters, null);

            SearchClient.Post(doc);
            SearchClient.Commit();
            SearchClient.Close();

            return doc;
        }


        public T Update(ContentCrawlProxy parameters)
        {
            var doc = CreateSearchDoc(parameters, null);

            SearchClient.Post(doc);
            SearchClient.Commit();
            SearchClient.Close();

            return doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        public int Delete(IEnumerable<ISearchableAsset> assets)
        {
            var items = assets as IList<ISearchableAsset> ?? assets.ToList();

            foreach (var asset in items)
            {
                SearchClient.DeleteById(asset._ContentID);
            }

            return items.Count();
        }

               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public void Delete(ISearchableAsset asset)
        {
            SearchClient.DeleteById(asset._ContentID);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private T CreateSearchDoc(ContentCrawlProxy parameters, IndexResults results)
        {
            var doc = (T)Activator.CreateInstance(typeof(T), new object[] { });

            doc.id = parameters.ContentItem._ContentID;

            doc.sourceid = _sourceId;

            var docProps = doc.GetType().GetProperties();
            
            if (parameters.Content != null && parameters.Content.Any())
            {
                foreach (var crawlPropContent in parameters.Content)
                {
                    var docProp = docProps.FirstOrDefault(p => p.Name == crawlPropContent.Name);

                    if (docProp != null)
                    {
                        SetPropertyValue(doc, docProp, crawlPropContent.Value);
                    }
                }
            }
                        
            var baseType = parameters.ContentItem.GetType().BaseType;

            var pageBaseTypes = new List<Type>();

            while (baseType != null)
            {
                pageBaseTypes.Add(baseType);
                baseType = baseType.BaseType;
            }

            pageBaseTypes.Reverse();

            foreach (var bType in pageBaseTypes)
            {
                AddSearchIndexProperties(parameters.ContentItem, doc, docProps, bType);
            }
                       

            if (!((ISearchableAsset)parameters.ContentItem).DisableExtract)
            {
                try
                {
                    var blob = ((ISearchableAsset)parameters.ContentItem).AssetBlob;

                    if (blob != null && blob.Length <= Threshold)
                    {
                        //doc = SearchClient.Extract(doc, blob);
                        var responseXml = SearchClient.FileExtract(blob);

                        var xmlParser = new XmlParser(responseXml);
                        var xhtml = xmlParser.ParseHTML("/response/str");
                        var htmlParser = new HtmlParser(WebUtility.HtmlDecode(xhtml));

                        //doc.mimetype = xmlParser.ParseString("/response/lst/arr[@name='Content-Type']/str");
                        //var pubdate = xmlParser.ParseDate("/response/lst/arr[@name='Creation-Date']/str");

                        //if (pubdate != null)
                        //   doc.timestamp = pubdate.Value;

                        if (doc.content == null)
                            doc.content = new List<string>();

                        doc.content.Add(WebUtility.HtmlEncode(htmlParser.ParseStripInnerHtml("//body")));
                    }
                }
                catch(Exception ex)
                {
                    LogWarning(string.Format("Extraction failed for ID: {0} NAME:{1}. {2}", parameters.ContentItem._ContentID, parameters.ContentItem.Name, ex.Message));
                        
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
            var indexedAssets = SearchClient.Search("sourceid:" + _sourceId).Results;

            var notFound = indexedAssets
                                .Where(p => !assets.Any(pg => pg._ContentID == p.id))
                                .ToList();

            foreach (var page in notFound)
            {
                SearchClient.Delete("id:" + page.id);
            }

            SearchClient.Commit();

            return notFound.Count();
        }
    }
}
