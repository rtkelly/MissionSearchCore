using MissionSearch.Clients;
using MissionSearch.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using MissionSearch.Attributes;
using System.Web;
using System.Reflection;

namespace MissionSearch.Indexers
{
    public class DefaultContentIndexer<T> : IndexerBase<T>, IContentIndexer<T> where T : ISearchDocument 
    {
        public ISearchClient<T> SearchClient { get; set; }

        int SourceId; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchClient"></param>
        public DefaultContentIndexer(ISearchClient<T> srchClient, int sourceId)
        {
            if (srchClient == null)
                throw new NotImplementedException("Solr Client not implemented");

            SearchClient = srchClient;
            SourceId = sourceId; 

            _logger = SearchFactory<T>.Logger;
        }

        public DefaultContentIndexer(ISearchClient<T> srchClient, int sourceId, ILogger logger)
        {
            if (srchClient == null)
                throw new NotImplementedException("Solr Client not implemented");

            SearchClient = srchClient;
            SourceId = sourceId;

            _logger = logger;
        }

        public IndexResults RunFullIndex(IEnumerable<ISearchableContent> pages)
        {
            var results = RunUpdate(pages, null, null);

            results.DeleteCnt = PurgeDeletedDocuments(pages);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        /// <returns></returns>
        public IndexResults RunFullIndex(IEnumerable<ISearchableContent> pages, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var results = RunUpdate(pages, statusCallback, indexerCallback);

            results.DeleteCnt = PurgeDeletedDocuments(pages);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pages"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        public IndexResults RunUpdate(IEnumerable<ISearchableContent> pages, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var results = new IndexResults();

            results.TotalCnt = 0;
            results.ErrorCnt = 0;

            int cnt = 0;

            //SearchClient.Reload();

            foreach(var page in pages)
            {
                try
                {
                    if (page.NotSearchable)
                    {
                        if(SearchClient.Search("id:" + page.SearchId).Results.Any())
                        {
                            SearchClient.Delete("id:" + page.SearchId);
                            results.DeleteCnt++;
                        }
                        continue;
                    }
                    
                    var doc = CreateSearchDoc(page);

                    if (doc == null)
                        throw new Exception("error creating solr document");

                    results.TotalCnt++;

                    if (indexerCallback != null)
                    {
                        doc = indexerCallback(doc, page);
                    }

                    SearchClient.Post(doc);

                    if (statusCallback != null)
                    {
                        if (!statusCallback())
                        {
                            results.Stopped = true;
                            return results;
                        }
                    }
                                        
                    if (++cnt == 1000)
                    {
                        SearchClient.PostCommit();
                        cnt = 0;
                        //System.Threading.Thread.Sleep(5000);

                    }
                }
                catch(Exception ex)
                {
                    LogError(string.Format("Indexing failed for \"{0}\". {1} {2}", page.Name, ex.Message, ex.StackTrace));
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
        /// <param name="pages"></param>
        private int PurgeDeletedDocuments(IEnumerable<ISearchableContent> pages)
        {
            var indexedPages = SearchClient.SearchAll("sourceid:" + SourceId);
            
            var notFound = indexedPages
                                .Where(p => !pages.Any(pg => pg.SearchId == p.id))
                                .ToList();

            foreach(var page in notFound)
            {
                SearchClient.DeleteById(page.id);
            }

            SearchClient.PostCommit();

            return notFound.Count();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public T Update(ISearchableContent page)
        {
            var doc = CreateSearchDoc(page);

            if (page.NotSearchable)
            {
                SearchClient.DeleteById(doc.id);
            }
            else
            {
                SearchClient.Post(doc);
            }

            SearchClient.PostCommit();
            SearchClient.Close();

            return doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pages"></param>
        /// <returns></returns>
        public int Delete(IEnumerable<ISearchableContent> pages)
        {
            foreach(var page in pages)
            {
                SearchClient.DeleteById(page.SearchId);
            }

            return pages.Count();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public void Delete(ISearchableContent page)
        {
            SearchClient.DeleteById(page.SearchId);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private T CreateSearchDoc(ISearchableContent page)
        {
            var doc = (T)Activator.CreateInstance(typeof(T), new object[] { });
            
            doc.sourceid = SourceId; 
            doc.id = page.SearchId;
            doc.url = page.SearchUrl;
            doc.title = page.Name;
            doc.timestamp = page.Changed;

            var pageProps = page.GetType().GetProperties();
            var docProps = doc.GetType().GetProperties();

            var pageCrawlProps = page.CrawlProperties as CrawlerContentSettings;

            if (pageCrawlProps != null && pageCrawlProps.Content != null && pageCrawlProps.Content.Any())
            {
                foreach(var crawlPropContent in pageCrawlProps.Content)
                {
                    var docProp = docProps.FirstOrDefault(p => p.Name == crawlPropContent.Name);

                    if(docProp != null)
                    {
                        SetPropertyValue(doc, docProp, crawlPropContent.Value);
                    }
                }
            }

            var pageBaseTypes = new List<System.Type>();

            pageBaseTypes.Add(page.GetType());
            
            var baseType = page.GetType().BaseType;

            while (baseType != null)
            {
                pageBaseTypes.Add(baseType);
                baseType = baseType.BaseType;
            }

            pageBaseTypes.Reverse();

            foreach (var bType in pageBaseTypes)
            {
                GetBaseProperties(page, doc, docProps, bType);
            }

            var docContent = doc.content != null ? HtmlParser.StripHTML(string.Join(" ", doc.content)) : "";

            doc.highlightsummary = (HtmlParser.StripHTML(doc.summary) + " " + docContent + " " + doc.title).Trim();
            
            
            return doc;
        }

        

        
    }
}
