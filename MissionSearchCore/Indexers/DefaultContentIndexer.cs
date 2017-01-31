using MissionSearch.Clients;
using MissionSearch.Util;
using System;
using System.Linq;
using System.Collections.Generic;

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
        /// <param name="sourceId"></param>
        public DefaultContentIndexer(ISearchClient<T> srchClient, int sourceId)
        {
            if (srchClient == null)
                throw new NotImplementedException("Solr Client not implemented");

            SearchClient = srchClient;
            SourceId = sourceId; 

            _logger = SearchFactory.Logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchClient"></param>
        /// <param name="sourceId"></param>
        /// <param name="logger"></param>
        public DefaultContentIndexer(ISearchClient<T> srchClient, int sourceId, ILogger logger)
        {
            if (srchClient == null)
                throw new NotImplementedException("Solr Client not implemented");

            SearchClient = srchClient;
            SourceId = sourceId;

            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public IndexResults RunFullIndex(IEnumerable<ISearchableContent> content)
        {
            return RunFullIndex(content, null, null);
        }

        public IndexResults RunFullIndex(IEnumerable<ContentCrawlParameters> content)
        {
            return RunFullIndex(content, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        /// <returns></returns>
        public IndexResults RunFullIndex(IEnumerable<ContentCrawlParameters> content, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var contentList = content as IList<ContentCrawlParameters> ?? content.ToList();
            var contentItems = contentList.Select(c => c.ContentItem).ToList();

            var results = RunUpdate(contentList, statusCallback, indexerCallback);

            results.DeleteCnt = PurgeDeletedDocuments(contentItems);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        /// <returns></returns>
        public IndexResults RunFullIndex(IEnumerable<ISearchableContent> content, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var contentList = content as IList<ISearchableContent> ?? content.ToList();
            var contentParamters = contentList.Select(c => new ContentCrawlParameters() { ContentItem = c }).ToList();
            
            var results = RunUpdate(contentParamters, statusCallback, indexerCallback);

            results.DeleteCnt = PurgeDeletedDocuments(contentList);

            return results;
        }

       
        public IndexResults RunUpdate(IEnumerable<ISearchableContent> contentItems, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var parameters = contentItems.Select(c => new ContentCrawlParameters() { ContentItem = c });

            return RunUpdate(parameters, statusCallback, indexerCallback);
        }
       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItems"></param>
        /// <param name="statusCallback"></param>
        /// <param name="indexerCallback"></param>
        public IndexResults RunUpdate(IEnumerable<ContentCrawlParameters> contentItems, Global<T>.StatusCallBack statusCallback, Global<T>.IndexCallBack indexerCallback)
        {
            var results = new IndexResults
            {
                TotalCnt = 0,
                ErrorCnt = 0
            };
            
            int cnt = 0;
            
            if (contentItems == null)
                return results;

            foreach(var parameters in contentItems)
            {
                try
                {
                    if (parameters.ContentItem.NotSearchable)
                    {
                        if(SearchClient.Search("id:" + parameters.ContentItem._ContentID + " AND sourceid:" + SourceId.ToString()).Results.Any())
                        {
                            SearchClient.Delete("id:" + parameters.ContentItem._ContentID);
                            results.DeleteCnt++;
                        }
                        continue;
                    }
                   
                    var doc = CreateSearchDoc(parameters);

                    if (doc == null)
                        throw new Exception("error creating solr document");

                    results.TotalCnt++;

                    if (indexerCallback != null)
                    {
                        doc = indexerCallback(doc, parameters.ContentItem);
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

                    if (++cnt != 1000) 
                        continue;

                    SearchClient.Commit();
                    cnt = 0;
                    //System.Threading.Thread.Sleep(5000);
                }
                catch(Exception ex)
                {
                    LogError(string.Format("Indexing failed for \"{0}\". {1} {2}", parameters.ContentItem.Name, ex.Message, ex.StackTrace));
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
        /// <param name="contentItems"></param>
        private int PurgeDeletedDocuments(IEnumerable<ISearchableContent> contentItems)
        {
            var indexedPages = SearchClient.GetAll("sourceid:" + SourceId);
            
            var notFound = indexedPages
                                .Where(p => contentItems.All(pg => pg._ContentID != p.id))
                                .ToList();

            foreach(var page in notFound)
            {
                SearchClient.DeleteById(page.id);
            }

            SearchClient.Commit();

            return notFound.Count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public T Update(ContentCrawlParameters contentItem)
        {
            var doc = CreateSearchDoc(contentItem);

            if (contentItem.ContentItem.NotSearchable)
            {
                SearchClient.DeleteById(doc.id);
            }
           else
            {
                SearchClient.Post(doc);
            }

            SearchClient.Commit();
            SearchClient.Close();

            return doc;
        }

        public T Update(ISearchableContent contentItem)
        {
            return Update(new ContentCrawlParameters() { 
                ContentItem = contentItem 
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItems"></param>
        /// <returns></returns>
        public int Delete(IEnumerable<ISearchableContent> contentItems)
        {
            var items = contentItems as IList<ISearchableContent> ?? contentItems.ToList();

            foreach(var page in items.ToList())
            {
                SearchClient.DeleteById(page._ContentID);
            }

            return items.Count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public void Delete(ISearchableContent contentItem)
        {
            SearchClient.DeleteById(contentItem._ContentID);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        private T CreateSearchDoc(ContentCrawlParameters contentItem)
        {
            if (contentItem == null)
                return default(T);

            var doc = (T)Activator.CreateInstance(typeof(T), new object[] { });

            doc.sourceid = SourceId;

            var docProps = doc.GetType().GetProperties();

            if (contentItem.ContentItem != null)
            {
                doc.id = contentItem.ContentItem._ContentID;
                //doc.title = contentItem.ContentItem.Name;
                
                var contentBaseTypes = ReflectionUtil.GetBaseTypes(contentItem.ContentItem);

                //contentBaseTypes.Add(contentItem.GetType());

                foreach (var bType in contentBaseTypes)
                {
                    GetBaseProperties(contentItem.ContentItem, doc, docProps, bType);
                }
            }
            
            //var contentCrawlProps = contentItem.CrawlProperties as ContentCrawlParameters;

            if (contentItem.Content != null && contentItem.Content.Any())
            {
                foreach (var crawlPropContent in contentItem.Content)
                {
                    var docProp = docProps.FirstOrDefault(p => p.Name == crawlPropContent.Name);

                    if (docProp != null)
                    {
                        SetPropertyValue(doc, docProp, crawlPropContent.Value);
                    }
                }
            }

            var docContent = doc.content != null ? HtmlParser.StripHTML(string.Join(" ", doc.content)) : "";
            doc.highlightsummary = (HtmlParser.StripHTML(doc.summary) + " " + docContent + " " + doc.title).Trim();
            
            return doc;
        }

        

        
    }
}
