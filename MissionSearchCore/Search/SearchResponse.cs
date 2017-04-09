using MissionSearch.Clients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MissionSearch
{
    public class SearchResponse
    {
        public string QueryString { get; set; }

        public string JsonResponse { get; set; }

        internal SolrResponseContainer ResponseContainer { get; set; }

        public List<dynamic> Results { get; set; }

        public List<Refinement> Refinements;

        public int PageSize { get; set; }

        internal int CurrentPage { get; set; }

        public int TotalFound { get; set; }

        public string SearchText { get; set; }

        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
        

    }
    public class SearchResponse<T> : SearchResponse
    {
        public new List<T> Results { get; set; }
        
               
        internal int TotalPages
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

        private int Start
        {
            get
            {
                return (CurrentPage - 1) * PageSize + 1;
            }
        }

        private int End
        {
            get
            {
                return (CurrentPage - 1) * PageSize + (PageSize-1);
            }
        }

        private Pagination _pagingInfo { get; set; }

        public Pagination PagingInfo
        {
            get
            {
                if (_pagingInfo == null)
                {
                   
                    _pagingInfo = BuildPagination();
                }

                return _pagingInfo;
            }
        }


        internal SolrResponseContainer<T> ResponseContianer
        {
            get
            {
                return  JsonConvert.DeserializeObject<SolrResponseContainer<T>>(JsonResponse, new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                });
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
        /// <param name="response"></param>
        /// <param name="request"></param>
        /// <param name="postBackUrl"></param>
        /// <returns></returns>
        private Pagination BuildPagination()
        {
            var pagination = new Pagination();
            pagination.Pages = new List<PaginationPage>();
            pagination.TotalPages = TotalPages;
            pagination.CurrentPage = CurrentPage;
            pagination.StartRow = Start;
            pagination.EndRow = (End > TotalFound) ? TotalFound  : End;
            pagination.TotalRows = TotalFound;
                        
            int groupSize = 8;

            if (TotalPages <= 1) return pagination;

            int groupHalf = groupSize / 2;

            int pageStart = (CurrentPage - groupHalf);
            int pageEnd = (CurrentPage + groupHalf);

            if (pageEnd >= TotalPages)
            {
                pageStart = pageStart - (pageEnd - TotalPages);
                pageEnd = TotalPages;
            }

            if (pageStart < 1)
            {
                pageEnd = pageEnd - pageStart;
                pageStart = 1;
            }

            if (pageEnd >= TotalPages)
            {
                pageEnd = TotalPages;
            }

            int prevPage = CurrentPage - 1;
            int nextPage = (CurrentPage + 1) > TotalPages ? TotalPages : CurrentPage + 1;

            if (prevPage < 1)
                prevPage = 1;
            
            pagination.FirstPage = 1;
            pagination.LastPage = pageEnd;
            pagination.PrevPage = prevPage;
            pagination.NextPage = (CurrentPage + 1) > TotalPages ? pageEnd : CurrentPage + 1;

            pagination.FirstPageLink = "&page=1";
            pagination.LastPageLink = string.Format("&page={0}", TotalPages);
            pagination.PrevPageLink = string.Format("&page={0}", prevPage);
            pagination.NextPageLink = string.Format("&page={0}", nextPage);

            for (int pageCnt = pageStart; pageCnt <= pageEnd; pageCnt++)
            {
                pagination.Pages.Add(new PaginationPage
                {
                    Page = pageCnt,
                    Link = string.Format("&page={0}", pageCnt),
                    Current = (CurrentPage == pageCnt),
                });
            }
                       

            return  pagination;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<RefinementItem> GetSelectedRefinements()
        {
            var selected = new List<RefinementItem>();

            foreach (var r in Refinements)
            {
                foreach (var i in r.Items)
                {
                    if (i.Selected)
                        selected.Add(i);
                }
            }

            return selected;
        }




    }
}