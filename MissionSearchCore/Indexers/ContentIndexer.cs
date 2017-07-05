using MissionSearch.Clients;
using MissionSearch.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

namespace MissionSearch.Indexers
{
    public class ContentIndexer : IndexerBase, IContentIndexer
    {
        public ISearchClient SearchClient { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="constr"></param>
        /// <param name="sourceId"></param>
        /// <param name="logger"></param>
        public ContentIndexer(string constr, int sourceId, ILogger logger)
        {
            SearchClient = new SolrClient(constr);
            
            _sourceId = sourceId;

            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItems"></param>
        /// <returns></returns>
        public int Delete(List<ISearchableContent> contentItems)
        {
            var items = contentItems as IList<ISearchableContent> ?? contentItems.ToList();

            foreach (var page in items.ToList())
            {
                SearchClient.DeleteById(page._ContentID);
            }

            return items.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItem"></param>
        public void Delete(ISearchableContent contentItem)
        {
            SearchClient.DeleteById(contentItem._ContentID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentList"></param>
        /// <returns></returns>
        public IndexResults RunFullIndex(List<ContentCrawlParameters> contentList)
        {
            var contentItems = contentList.Select(c => c.ContentItem).ToList();

            var results = RunUpdate(contentList);

            results.DeleteCnt = PurgeDeletedDocuments(contentItems);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItems"></param>
        /// <returns></returns>
        public IndexResults RunUpdate(List<ContentCrawlParameters> contentItems)
        {
            var results = new IndexResults
            {
                TotalCnt = 0,
                ErrorCnt = 0
            };

            var cnt = 0;

            if (contentItems == null)
                return results;

            foreach (var contentItem in contentItems)
            {
                try
                {
                    var doc = CreateSearchJsonDoc(contentItem.ContentItem._ContentID, contentItem);

                    results.TotalCnt++;

                    SearchClient.Post(doc);

                    if (++cnt != 1000)
                        continue;

                    SearchClient.Commit();
                }
                catch (Exception ex)
                {
                    LogError(string.Format("Indexing failed for \"{0}\". {1} {2}", contentItem.ContentItem.Name, ex.Message, ex.StackTrace));
                    results.ErrorCnt++;
                }
            }

            if (contentItems.Any())
            {
                SearchClient.Commit();
                SearchClient.Close();
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="crawlProperties"></param>
        /// <returns></returns>
        public string CreateSearchJsonDoc(string id, ContentCrawlParameters crawlProperties)
        {
            if (crawlProperties == null)
                return null;

            crawlProperties.Content.Insert(0, new CrawlerContent()
            {
                Value = id,
                Name = "id",
            });

            crawlProperties.Content.Insert(1, new CrawlerContent()
            {
                Value = _sourceId,
                Name = "sourceid",
            });

            var dict = new Dictionary<string, object>();

            var propCnts = from x in crawlProperties.Content
                           group x by x.Name into g
                           let count = g.Count()
                           orderby count descending
                           select new { Name = g.Key, Count = count };

            foreach (var propCnt in propCnts)
            {
                if (propCnt.Count > 1)
                {
                    var values = crawlProperties.Content
                        .Where(p => p.Name == propCnt.Name && p.Value != null)
                        .Select(p => p.Value)
                        .ToList();

                    dict.Add(propCnt.Name, values);
                }
                else
                {
                    var crawlProp = crawlProperties.Content.FirstOrDefault(p => p.Name == propCnt.Name);

                    if (crawlProp != null && crawlProp.Value != null)
                    {
                        var isEnumerable = crawlProp.Value as IList;
                        
                        if (isEnumerable != null)
                        {
                            if (isEnumerable.Count > 0)
                            {
                                dict.Add(propCnt.Name, crawlProp.Value);
                            }
                        }
                        else
                        {
                            dict.Add(propCnt.Name, crawlProp.Value);    
                        }
                    }
                }
            }

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };

            return JsonConvert.SerializeObject(dict, settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentItems"></param>
        private int PurgeDeletedDocuments(IEnumerable<ISearchableContent> contentItems)
        {
            var indexedPages = SearchClient.GetAll("sourceid:" + _sourceId);

            var notFound = indexedPages
                                .Where(p => contentItems.All(pg => pg._ContentID != p.id))
                                .ToList();

            foreach (var page in notFound)
            {
                SearchClient.DeleteById(page.id);
            }

            SearchClient.Commit();

            return notFound.Count;
        }


    }

}
