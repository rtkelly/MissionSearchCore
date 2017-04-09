using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MissionSearch.Search.Refinements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace MissionSearch.Clients
{

    public class LuceneClient : ISearchClient
    {
        string _srchConnStr;
        public string SrchConnStr { get { return _srchConnStr; } }

        public int Timeout { get; set; }


        protected IndexWriter _Writer;
        protected IndexWriter Writer
        {
            get
            {
                if (_Writer == null)
                    _Writer = GetWriter();

                return _Writer;
            }
        }

        public string SearchDefaultField = "content";

        protected Lucene.Net.Util.Version LuceneVer = Lucene.Net.Util.Version.LUCENE_30;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchConnectionString"></param>
        public LuceneClient(string srchConnectionString)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Lucene Index undefined");

            if (srchConnectionString.StartsWith("/"))
            {
                _srchConnStr = HostingEnvironment.MapPath(srchConnectionString);
            }
            else
            {
                _srchConnStr = srchConnectionString;
            }
        }

        /// <summary>
        /// returns lucene index writer
        /// </summary>
        /// <returns></returns>
        protected IndexWriter GetWriter()
        {
            var directory = FSDirectory.Open(SrchConnStr);
            var analyzer = new StandardAnalyzer(LuceneVer);
            
            return new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonDoc"></param>
        public void Post(string jsonDoc)
        {
            var doc = JsonConvert.DeserializeObject<dynamic>(jsonDoc, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });
            
            // to do: remove lucene object mapper
            // replace with json lucene doc mapper class

            var luceneDoc = doc.ToDocument();

            var key = new Term("id", doc.id);

            Writer.UpdateDocument(key, luceneDoc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public void Delete(string query)
        {
 	        throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SearchResponse Search(SearchRequest request)
        {
            var srchResponse = new SearchResponse();
            srchResponse.Results = new List<dynamic>();

            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(SrchConnStr));
            
            var reader = IndexReader.Open(directory, true);
            
            var searcher = new IndexSearcher(reader);

            var query = BuildQuery(searcher, request);
            //var filters = LoadFilters(request);
            var sortOrder = BuildSort(request);
            //var hits =  searcher.Search(query, filters, request.End, sortOrder);
            var hits = searcher.Search(query, null, request.End, sortOrder);

            srchResponse.TotalFound = hits.TotalHits;
            srchResponse.PageSize = request.PageSize;
            srchResponse.CurrentPage = request.CurrentPage;
            srchResponse.Refinements = LoadRefinements(searcher, request, query);

            foreach (var hitScore in hits.ScoreDocs.Skip(request.Start).Take(request.PageSize))
            {
                var scoreDoc = searcher.Doc(hitScore.Doc);

                var srchDoc = new LuceneDoc(scoreDoc);

                srchResponse.Results.Add(srchDoc);
            }
            
            srchResponse.SearchText = request.QueryText;

            return srchResponse;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected Query BuildQuery(IndexSearcher searcher, SearchRequest request)
        {
            var analyzer = new StandardAnalyzer(LuceneVer);

            var mainQuery = new StringBuilder();
            mainQuery.Append("+" + request.QueryText);

            var filters = LoadFilters(request);

            if (!string.IsNullOrEmpty(filters))
            {
                mainQuery.Append(filters);
            }

            var parser = new QueryParser(LuceneVer, SearchDefaultField, analyzer);
            
            var query = parser.Parse(mainQuery.ToString());


            return query;
        }


        protected Sort BuildSort(SearchRequest request)
        {
            var sortOrder = new Sort();

            if (!request.Sort.Any())
            {
                sortOrder.SetSort(new SortField("", SortField.SCORE));
            }
            else
            {
                var sorts = new List<SortField>();

                foreach (var sort in request.Sort)
                {
                    var order = (sort.Order == SortOrder.SortOption.Ascending) ? false : true;

                    sorts.Add(new SortField(sort.SortField, SortField.STRING, order));
                }

                sortOrder = new Sort(sorts.ToArray());

            }

            return sortOrder;
        }
         
        /*
        protected BooleanFilter LoadFilters(SearchRequest request)
        {
            var filters = new BooleanFilter();
            var cnt = 0;

            foreach (var filter in request.QueryOptions.OfType<FilterQuery>())
            {
                if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Equals)
                {
                    var queryFilter = new QueryWrapperFilter(new TermQuery(new Term(filter.ParameterName, filter.FieldValue.ToString())));
                    filters.Add(new FilterClause(queryFilter, Occur.MUST));
                    cnt++;
                }
                else if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Contains)
                {
                    var queryFilter = new QueryWrapperFilter(new WildcardQuery(new Term(filter.ParameterName, "*" + filter.FieldValue + "*")));
                    filters.Add(new FilterClause(queryFilter, Occur.MUST));
                    cnt++;
                }
            }

            if (!string.IsNullOrEmpty(request.Refinements))
            {
                var currentRefinements = QueryOptions.ParseRefinementString(request.Refinements);

                foreach (var refinement in currentRefinements)
                {
                    var terms = new string[] { refinement.FieldValue.ToString() };

                    filters.Add(new FilterClause(new FieldCacheTermsFilter(refinement.ParameterName, terms), Occur.MUST));
                    cnt++;
                }
            }

            if (cnt == 0)
                filters = null;

            return filters;
        }
         */

        protected string LoadFilters(SearchRequest request)
        {
            var filters = new StringBuilder();
            
            foreach (var filter in request.QueryOptions.OfType<FilterQuery>())
            {
                if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Equals)
                {
                    filters.Append(string.Format(" +{0}:{1}", filter.ParameterName, filter.FieldValue));
                }
                else if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Contains)
                {
                    filters.Append(string.Format(" +{0}:*{1}*", filter.ParameterName, filter.FieldValue));
                }
            }

            if (!string.IsNullOrEmpty(request.Refinements))
            {
                var currentRefinements = QueryOptions.ParseRefinementString(request.Refinements);

                foreach (var refinement in currentRefinements)
                {
                    filters.Append(string.Format(" +{0}:{1}", refinement.ParameterName, refinement.FieldValue));
                }
            }

            return filters.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected List<Refinement> LoadRefinements(IndexSearcher searcher, SearchRequest request, Query query)
        {
            var refinements = new List<Refinement>();

            var currentFilterQueries = QueryOptions.ParseRefinementString(request.Refinements);

            foreach (var facet in request.Facets)
            {
                var sfs = new SimpleFacetedSearch(searcher.IndexReader, facet.FieldName);
                var hits = sfs.Search(query);

                var refinement = new Refinement()
                {
                    Name = facet.FieldName,
                    Label = facet.FieldLabel,
                    Items = new List<RefinementItem>(),
                };

                refinements.Add(refinement);

                foreach (var hit in hits.HitsPerFacet)
                {
                    if (hit.HitCount == 0)
                        continue;

                    var item = new RefinementItem()
                    {
                        Name = refinement.Name,
                        DisplayName = hit.Name.ToString(),
                        Count = hit.HitCount,
                        Value = hit.Name.ToString(),
                        Selected = currentFilterQueries.Any(f => f.FieldValue.ToString().Contains(string.Format("{0}", hit.Name.ToString()))),
                        
                    };

                    item.Refinement = RefinementBuilder.AddRemoveRefinement(item, request.Refinements, facet.RefinementOption);
                    item.Link = string.Format("&ref={0}", item.Refinement);
                    
                    refinement.Items.Add(item);
                }

                refinement.Items = refinement.Items.OrderByDescending(p => p.Count).ToList();
            }

            return refinements; 
        }

        /// <summary>
        /// 
        /// </summary>
        public void Commit()
        {
            //if (_Writer != null)
           /// {
            //    Writer.Commit();
           // }
        }


        /// <summary>
        /// Intialization after posting is completed
        /// </summary>
        public void PostCommit()
        {
            if (_Writer != null)
            {
                Writer.Optimize();
                Writer.Commit();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            if (_Writer != null)
            {
                Writer.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void DeleteById(string id)
        {
            var term = new Term("id", id);
            Writer.DeleteDocuments(term);
            Writer.Optimize();
            Writer.Commit();
            Writer.Dispose();
        }

        /// <summary>
        /// Intialization before posting begins
        /// </summary>
        public void PostInit()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<string> GetTerms(string fieldName, string term)
        {
            throw new NotImplementedException();
        }

    }

    public class LuceneClient<T> : LuceneClient, ISearchClient<T> where T : ISearchDocument 
    {

        /// <summary>
        /// /
        /// </summary>
        /// <param name="srchConnectionString"></param>
        public LuceneClient(string srchConnectionString) : base(srchConnectionString)
        {
          
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public SearchResponse<T> Search(string queryText)
        {
            return Search(new SearchRequest()
            {
                QueryText = queryText,
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public new SearchResponse<T> Search(SearchRequest request)
        {
            var srchResponse = new SearchResponse<T>();
            srchResponse.Results = new List<T>();

            var directory = FSDirectory.Open(new System.IO.DirectoryInfo(SrchConnStr));
            var searcher = new IndexSearcher(IndexReader.Open(directory, true));

            var query = BuildQuery(searcher, request);
            //var filters = LoadFilters(request);
            var sortOrder = BuildSort(request);
            //var hits = searcher.Search(query, filters, request.End, sortOrder);
            var hits = searcher.Search(query, null, request.End, sortOrder);

            srchResponse.TotalFound = hits.TotalHits;
            srchResponse.PageSize = request.PageSize;
            srchResponse.CurrentPage = request.CurrentPage;
            srchResponse.Refinements = LoadRefinements(searcher, request, query);
                        
            var luceneMapper = new LuceneMapper<T>();

            foreach (var hitScore in hits.ScoreDocs.Skip(request.Start).Take(request.PageSize))
            {
                var scoreDoc = searcher.Doc(hitScore.Doc);
                var srchDoc = luceneMapper.ToSearchDocument(scoreDoc);

                srchResponse.Results.Add(srchDoc);
            }

            srchResponse.SearchText = request.QueryText;

            return srchResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public BooleanClause CreateFilteredQuery(string fieldName, string value)
        {
            //BooleanQuery bq = new BooleanQuery();
            var term = new Term(fieldName, value);
            
            var bc = new BooleanClause(new PrefixQuery(term), Occur.MUST);

            //bq.Add(bc);
            
            return bc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void Post(T doc)
        {
            //var luceneDoc = doc.ToDocument();
            
            var luceneMapper = new LuceneMapper<T>();

            var luceneDoc = luceneMapper.ToLuceneDocument(doc);

            var idTerm = new Term("id", luceneDoc.GetField("id").StringValue);

            Writer.UpdateDocument(idTerm, luceneDoc);
        }
        
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public List<T> GetAll(string q)
        {
            var indexedPages = new List<T>();

            var response = Search(new SearchRequest()
            {
                QueryText = q,
                PageSize = 500,
            });

            if (response.Results.Any())
                indexedPages.AddRange(response.Results);

            for (int page = 2; page <= response.PagingInfo.TotalPages; page++)
            {
                response = Search(new SearchRequest()
                {
                    QueryText = q,
                    CurrentPage = page,
                    PageSize = 500,
                });

                if (response.Results.Any())
                    indexedPages.AddRange(response.Results);
            }

            return indexedPages;
        }


        public string Extract(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }
                

        

        public string FileExtract(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }
      




        
    }
}
