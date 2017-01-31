using System.Collections.Generic;

namespace MissionSearch.Clients
{
    public interface ISearchClient<T> where T : ISearchDocument 
    {
        int Timeout { get; set; }

        string SrchConnStr { get;  }

        SearchResponse<T> Search(SearchRequest request);
        
        SearchResponse<T> Search(string queryText);

        List<T> SearchAll(string queryText);

        T Extract(T doc, byte[] fileBytes);
        
        void Post(T doc);
        
        void DeleteById(string id);

        void Delete(string query);
        

        List<string> GetTerms(string fieldName, string term);
            
        void PostInit();
        
        void PostCommit();

        void Close();

        void Reload();
        
        List<string> GetSynonyms();

        //IAutoCompleteClient<T> QueryIndexer { get; set; }

    }
}
