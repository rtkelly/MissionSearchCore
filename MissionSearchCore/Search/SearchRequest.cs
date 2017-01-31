using MissionSearch.Search;
using MissionSearch.Search.Facets;
using MissionSearch.Search.Query;
using MissionSearch.Suggester;
using MissionSearch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MissionSearch
{
    public class SearchRequest
    {
        public string QueryText { get; set; }
        
        public int CurrentPage { get; set; }
        
        public int PageSize { get; set; }
        
        public List<SortOrder> Sort { get; set; }
        
        public List<IQueryOption> QueryOptions { get; set; }
                
        public List<IFacet> Facets { get; set; }
        
        public string Refinements { get; set; }
                
        //public bool SingleSelectRefinement { get; set; }
                
        public RefinementTypes RefinementType { get; set; }

        public string Language { get; set; }

        public IQuerySuggester QueryIndexer { get; set; }

        public bool EnableHighlighting { get; set; }

        public bool EnableQueryLogging { get; set; }
        

        public SearchRequest()
        {
            Facets = new List<IFacet>();
            QueryOptions = new List<IQueryOption>();
            Sort = new List<SortOrder>();
            PageSize = 50;
            CurrentPage = 1;
            RefinementType = RefinementTypes.Refinement;
            
        }
                

        public int Start
        {
            get
            {
                return (CurrentPage - 1) * PageSize;
            }
        }

        public int End
        {
            get
            {
                return (CurrentPage - 1) * PageSize + PageSize;
            }
        }
        
                

        
       
    }
}