using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissionSearch.Clients;
using System.Web;

namespace MissionSearch.Suggester
{
    public class QuerySuggester : IQuerySuggester
    {
       public ISearchClient<QuerySuggesterDocument> SrchClient;

       public enum Provider {
           Lucene,
           Solr,
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchClient"></param>
       public QuerySuggester(ISearchClient<QuerySuggesterDocument> srchClient)
       {
            SrchClient = srchClient;
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchConnectionString"></param>
        /// <param name="provider"></param>
       public QuerySuggester(string srchConnectionString, Provider provider)
       {
           switch(provider)
           {
               case Provider.Lucene:
                   SrchClient = new LuceneClient<QuerySuggesterDocument>(srchConnectionString);
                   break;

               case Provider.Solr:
                   SrchClient = new SolrClient<QuerySuggesterDocument>(srchConnectionString);
                   break;

           }
           //Client = srchClient;
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="minimumFrequency"></param>
        /// <returns></returns>
       public List<string> GetMatches(string term, int minimumFrequency= 1)
       {
           var resp = TermSearch(term, 1, 5);

           var results = resp.Results
               //.Where(r => r.hitcount >= minimumFrequency)
               .Select(r => r.title).ToList();

           return results.OrderBy(t => t).ToList();
       }

       public SearchResponse<QuerySuggesterDocument> TermSearch(string term, int page, int pagesize=100)
       {
           var req = new SearchRequest()
           {
               QueryText = "*:*",
               PageSize = pagesize,
               CurrentPage = page,
               Sort = new List<SortOrder>()
               {
                   new SortOrder("title", SortOrder.SortOption.Ascending),
               },
           };

           if(!string.IsNullOrEmpty(term))
           {
               req.QueryOptions.Add(new FilterQuery("title", FilterQuery.ConditionalTypes.Contains, term.Trim().ToLower()));
           }

           var resp = SrchClient.Search(req);

           return resp;
           //return resp.Results
             //  .Select(r => r.title).ToList();
       }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
       public ISearchDocument GetTerm(string term)
       {
           var req = new SearchRequest()
           {
               QueryText = term,
           };

           var resp = SrchClient.Search(req);

           var result = resp.Results.FirstOrDefault(r => r.id == term.ToLower());

           return result;
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
       public void AddUpdateTerm(string term, string language)
       {
           var doc = GetTerm(term) as QuerySuggesterDocument;

           if (doc == null)
           {
               AddTerm(term, language);
           }
           else
           {
               doc.hitcount = doc.hitcount + 1;
               doc.timestamp = DateTime.Now;
               SrchClient.Post(doc);
           }

           
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
       public void AddTerm(string term, string language)
       {
           var doc = new QuerySuggesterDocument();

           doc.id = string.Format("{0}_{1}", term.ToLower().Trim(), language);
           doc.title = term.ToLower().Trim();
           doc.content = new List<string>();
           doc.timestamp = DateTime.Now;
           doc.summary = "";
           doc.language = new List<string>();
           doc.language.Add(language);
           doc.url = "";
           doc.contenttype = "";
           doc.hitcount = 1;

           SrchClient.Post(doc);
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
       public void RemoveTerm(string term, string language)
       {
           SrchClient.DeleteById(string.Format("{0}_{1}", term, language));
       }


       /// <summary>
       /// 
       /// </summary>
       /// <returns></returns>
       public long GetIndexTotal()
       {
           var result = SrchClient.Search("*:*");
           return result.TotalFound;

       }


       /// <summary>
       /// 
       /// </summary>
       public void CommitTerms()
       {
           SrchClient.Commit();
       }


       /// <summary>
       /// 
       /// </summary>
       /// <returns></returns>
       public string GetConnectionString()
       {
           return SrchClient.SrchConnStr;
       }

       public string GetClientType()
       {
           return SrchClient.GetType().Name;
       }


       /// <summary>
       /// 
       /// </summary>
       public void Close()
       {
           SrchClient.Close();
       }
    }
}
