using MissionSearch.Clients;
using MissionSearch.Indexers;
using MissionSearch.Suggester;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public class SearchFactory
    {
        public static ICrawler Crawler
        {
            get
            {
                return ResolveComponent<ICrawler>();
            }
        }


        private static C ResolveComponent<C>(string id = null)
        {
            try
            {
                var container = DIContainers.GetWindsorContainer();
                return id == null ? container.Resolve<C>() : container.Resolve<C>(id);
            }
            catch
            {
                return default(C);
            }
        }

    }

    public class SearchFactory<T> where T : ISearchDocument
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IContentIndexer<T> ContentIndexer
        {
            get
            {
                return ResolveComponent<IContentIndexer<T>>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static IAssetIndexer<T> AssetIndexer
        {
            get
            {
                return ResolveComponent<IAssetIndexer<T>>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ISearchClient<T> SearchClient
        {
            get
            {
                return ResolveComponent<ISearchClient<T>>();
            }
        }

        public static ILogger Logger
        {
            get
            {
                return ResolveComponent<ILogger>();
            }
        }

        
              



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IQuerySuggester QuerySuggesterClient
        {
            get
            {
                return ResolveComponent<IQuerySuggester>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ISearchClient<T> GetSearchClient(string id)
        {
            return ResolveComponent<ISearchClient<T>>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IAssetIndexer<T> GetAssetIndexer(string id)
        {
            return ResolveComponent<IAssetIndexer<T>>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IContentIndexer<T> GetContentIndexer(string id)
        {
            return ResolveComponent<IContentIndexer<T>>(id);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ICrawler GetCrawler(string id)
        {
            return ResolveComponent<ICrawler>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        private static C ResolveComponent<C>(string id=null)
        {
            try
            {
                var container = DIContainers.GetWindsorContainer();
                return id == null ? container.Resolve<C>() : container.Resolve<C>(id);
            }
            catch
            {
                return default(C);
            }
        }
    }
}
