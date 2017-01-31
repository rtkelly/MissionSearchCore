using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MissionSearch
{
    public class SearchResponse<T>
    {
        public List<T> Results { get; set; }
        public List<Refinement> Refinements;

        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public string time { get; set; }
        public int TotalFound { get; set; }
        public string SearchText { get; set; }
        public string QueryString { get; set; }
        public string JsonResponse { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
        //public Dictionary<string, Dictionary<string, List<string>>> Highlighting { get; set; }
        
        public int TotalPages
        {
            get
            {
                if (TotalFound == 0 || PageSize == 0)
                    return 0;
                                
                int totalpages =  TotalFound / PageSize;

                if (TotalFound % PageSize != 0)
                    totalpages++;

                return totalpages;
            }
        }

        public int Start
        {
            get
            {
                return (CurrentPage - 1) * PageSize + 1;
            }
        }

        public int End
        {
            get
            {
                return (CurrentPage - 1) * PageSize + (PageSize-1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public SearchResponse()
        {
            Refinements = new List<Refinement>();
            Results = new List<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<RefinementItem> GetSelectedRefinements()
        {
            var selected = new List<RefinementItem>();

            foreach(var r in Refinements)
            {
                foreach(var i in r.Items)
                {
                    if (i.Selected)
                        selected.Add(i);
                }
            }

            return selected;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="request"></param>
        /// <param name="postBackUrl"></param>
        /// <returns></returns>
        public Pagination BuildPagination()
        {
            int totalPages = TotalPages;
            
            var currentPage = CurrentPage;
            
            var pagination = new Pagination();
            pagination.Pages = new List<PaginationPage>();
            pagination.TotalPages = totalPages;
            pagination.CurrentPage = currentPage;
            pagination.StartRow = Start;
            pagination.EndRow = (End > TotalFound) ? TotalFound  : End;
            pagination.TotalRows = TotalFound;
                        
            int groupSize = 8;

            if (totalPages <= 1) return pagination;

            int groupHalf = groupSize / 2;

            int pageStart = (currentPage - groupHalf);
            int pageEnd = (currentPage + groupHalf);

            if (pageEnd >= totalPages)
            {
                pageStart = pageStart - (pageEnd - totalPages);
                pageEnd = totalPages;
            }

            if (pageStart < 1)
            {
                pageEnd = pageEnd - pageStart;
                pageStart = 1;
            }

            if (pageEnd >= totalPages)
            {
                pageEnd = totalPages;
            }

            int prevPage = currentPage - 1;
            int nextPage = (currentPage + 1) > totalPages ? totalPages : currentPage + 1;

            if (prevPage < 1)
                prevPage = 1;
            
            pagination.FirstPage = 1;
            pagination.LastPage = pageEnd;
            pagination.PrevPage = prevPage;
            pagination.NextPage = (currentPage + 1) > totalPages ? pageEnd : currentPage + 1;


            pagination.FirstPageLink = "&page=1";
            pagination.LastPageLink = string.Format("&page={0}", totalPages);
            pagination.PrevPageLink = string.Format("&page={0}", prevPage);
            pagination.NextPageLink = string.Format("&page={0}", nextPage);

            for (int pageCnt = pageStart; pageCnt <= pageEnd; pageCnt++)
            {
                pagination.Pages.Add(new PaginationPage
                {
                    Page = pageCnt,
                    Link = string.Format("&page={0}", pageCnt),
                    Current = (currentPage == pageCnt),
                });
            }

            

            return  pagination;
        }

    }
}