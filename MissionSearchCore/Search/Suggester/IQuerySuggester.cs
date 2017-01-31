using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch.Suggester
{
    public interface IQuerySuggester
    {
        List<string> GetMatches(string term, int minimumFrequency);

        ISearchDocument GetTerm(string term);

        void AddUpdateTerm(string term, string language);
        void AddTerm(string term, string language);
        void CommitTerms();
        void Close();

        void RemoveTerm(string term, string language);

        long GetIndexTotal();
        string GetConnectionString();
        string GetClientType();

        SearchResponse<QuerySuggesterDocument> TermSearch(string term, int page, int pagesize = 100);
    }
}
