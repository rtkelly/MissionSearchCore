using System.Collections.Generic;

namespace MissionSearch.Clients
{
    public interface ISearchClient
    {
        int Timeout { get; set; }

        string SrchConnStr { get; }

        void Post(string jsonDoc);

        void Delete(string query);

        SearchResponse Search(SearchRequest request);
    }

    public interface ISearchClient<T> : ISearchClient where T : ISearchDocument 
    {
        //string SrchConnStr { get;  }

        new SearchResponse<T> Search(SearchRequest request);
        
        SearchResponse<T> Search(string queryText);

        List<T> GetAll(string queryText);

        string FileExtract(byte[] fileBytes);

        //T Extract(T doc, byte[] fileBytes);
        
        void Post(T doc);

        //void Post(string jsonDoc);
        
        void DeleteById(string id);

        List<string> GetTerms(string fieldName, string term);
            
        //void PostInit();
        
        void Commit();

        void Close();

        void Reload();
        
        //List<string> GetSynonyms();

        //IAutoCompleteClient<T> QueryIndexer { get; set; }

    }

   
}
