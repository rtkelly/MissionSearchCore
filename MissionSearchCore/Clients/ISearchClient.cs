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

        List<string> GetTerms(string fieldName, string term);

        string FileExtract(byte[] fileBytes);

        void Commit();

        void Close();

        void DeleteById(string id);

        void Reload();

        List<dynamic> GetAll(string queryText);
    }

    public interface ISearchClient<T> : ISearchClient where T : ISearchDocument 
    {
        new SearchResponse<T> Search(SearchRequest request);
        
        SearchResponse<T> Search(string queryText);

        new List<T> GetAll(string queryText);
        
        void Post(T doc);
        
        
        //List<string> GetSynonyms();

        //IAutoCompleteClient<T> QueryIndexer { get; set; }

    }

   
}
