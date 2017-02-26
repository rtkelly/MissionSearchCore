using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
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

            var hits = BaseSearch(searcher, request);
            
            srchResponse.TotalFound = hits.TotalHits;
            srchResponse.PageSize = request.PageSize;
            srchResponse.CurrentPage = request.CurrentPage;

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
        protected TopFieldDocs BaseSearch(IndexSearcher searcher, SearchRequest request)
        {
            var analyzer = new StandardAnalyzer(LuceneVer);

            var qstr = new StringBuilder();
            qstr.Append("+" + request.QueryText);

            var parser = new QueryParser(LuceneVer, SearchDefaultField, analyzer);
            var query = parser.Parse(qstr.ToString());

            // TO DO: implement sort from request object
            var sortField = new SortField("", SortField.SCORE);
            var sort = new Sort(sortField);

            /*
            if (!string.IsNullOrEmpty(request.Facets))
            {
                var rFields = refinements.Split(';');

                foreach (var rField in rFields)
                {
                    var fields = rField.Split('^');

                    if (fields.Count() != 2)
                        continue;

                    //var facetFilter = new FieldCacheTermsFilter(fields[0], fields[1]);
                    qstr.Append(string.Format(" +{0}:{1}", fields[0], fields[1]));
                }
            }
            */

            BooleanFilter boolFilter = null;

            foreach (var filter in request.QueryOptions.OfType<FilterQuery>())
            {
                if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Equals)
                {
                    if (boolFilter == null)
                        boolFilter = new BooleanFilter();

                    var queryFilter = new QueryWrapperFilter(new TermQuery(new Term(filter.ParameterName, filter.FieldValue.ToString())));
                    boolFilter.Add(new FilterClause(queryFilter, Occur.MUST));
                }
                else if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Contains)
                {
                    if (boolFilter == null)
                        boolFilter = new BooleanFilter();

                    var queryFilter = new QueryWrapperFilter(new WildcardQuery(new Term(filter.ParameterName, "*" + filter.FieldValue + "*")));
                    boolFilter.Add(new FilterClause(queryFilter, Occur.MUST));
                }
            }

            //var facetArray = request.Facets.Select(p => p.FieldName).ToArray;
            //var sfs = new SimpleFacetedSearch(searcher.IndexReader, facetArray);
            //var hits = sfs.Search(query, 10);

            return searcher.Search(query, boolFilter, request.End, sort);
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

            var hits = BaseSearch(searcher, request);
                        
            srchResponse.TotalFound = hits.TotalHits;
            srchResponse.PageSize = request.PageSize;
            srchResponse.CurrentPage = request.CurrentPage;
                        
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
