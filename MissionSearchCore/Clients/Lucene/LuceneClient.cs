using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace MissionSearch.Clients
{

    public class LuceneClient<T> : ISearchClient<T> where T : ISearchDocument 
    {
        string _srchConnStr;
        public string SrchConnStr { get { return _srchConnStr; } }

        public int Timeout { get; set; }

        string SearchDefaultField = "title";

        Lucene.Net.Util.Version LuceneVer = Lucene.Net.Util.Version.LUCENE_30;

        IndexWriter _Writer;
        IndexWriter Writer
        {
            get
            {
                if (_Writer == null)
                    _Writer = GetWriter();

                return _Writer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchConnectionString"></param>
        public LuceneClient(string srchConnectionString)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Solr Core undefined");

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
        public SearchResponse<T> Search(SearchRequest request)
        {
            var srchResponse = new SearchResponse<T>();
            srchResponse.Results = new List<T>();

            try
            {
                //var results = new List<Lucene.Net.Documents.Document>();

                var directory = FSDirectory.Open(new System.IO.DirectoryInfo(SrchConnStr));
                var searcher = new IndexSearcher(IndexReader.Open(directory, true));
                var analyzer = new StandardAnalyzer(LuceneVer);

                var qstr = new StringBuilder();
                qstr.Append("+" + request.QueryText);

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

                var parser = new QueryParser(LuceneVer, SearchDefaultField, analyzer);
                var query = parser.Parse(qstr.ToString());
                
                // TO DO: implement sort from request object
                var sortField = new SortField("title", SortField.STRING);
                var sort = new Sort(sortField);

                BooleanFilter boolFilter = null;

                //TermsFilter termsFilter = null;
                
                foreach(var filter in request.QueryOptions.OfType<FilterQuery>())
                {
                    if (filter.Condition == MissionSearch.FilterQuery.ConditionalTypes.Contains)
                    {
                        if (boolFilter == null)
                            boolFilter = new BooleanFilter();

                        var queryFilter = new QueryWrapperFilter(new WildcardQuery(new Term(filter.ParameterName, filter.FieldValue+"*")));
                        boolFilter.Add(new FilterClause(queryFilter, Occur.MUST));
                    }
                }
                                
                var hits = searcher.Search(query, boolFilter, request.End, sort);

                srchResponse.TotalFound = hits.TotalHits;
                srchResponse.PageSize = request.PageSize;
                srchResponse.CurrentPage = request.CurrentPage;

                Type t = typeof(T);
                var properties = t.GetProperties();

                foreach (var hitScore in hits.ScoreDocs.Skip(request.Start).Take(request.PageSize))
                {
                    var searchDoc = (T)Activator.CreateInstance(typeof(T), new object[] { });

                    var scoreDoc = searcher.Doc(hitScore.Doc);

                    var fields = scoreDoc.GetFields();

                    foreach (var field in fields)
                    {
                        try
                        {
                            var prop = properties.FirstOrDefault(p => p.Name == field.Name);

                            if (prop != null)
                            {
                                switch (prop.PropertyType.Name)
                                {
                                    case "DateTime":
                                        DateTime date;

                                        if (DateTime.TryParse(field.StringValue, out date))
                                            prop.SetValue(searchDoc, date);

                                        break;

                                    case "Int32":
                                        int i;

                                        if (int.TryParse(field.StringValue, out i))
                                            prop.SetValue(searchDoc, i);

                                        break;

                                    case "Int64":
                                        long l;

                                        if (long.TryParse(field.StringValue, out l))
                                            prop.SetValue(searchDoc, l);

                                        break;

                                    case "Boolean":
                                        Boolean b;

                                        if (Boolean.TryParse(field.StringValue, out b))
                                            prop.SetValue(searchDoc, b);

                                        break;

                                    default:
                                        prop.SetValue(searchDoc, field.StringValue.Trim());
                                        break;
                                }
                            }
                        }
                        catch
                        {
                            // TO DO: Handle errors
                        }
                    }


                    
                    srchResponse.Results.Add(searchDoc);
                }
            }
            catch
            {
                // TO DO: Handle errors
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
            var luceneDoc = new Document();
            //luceneDoc.Boost = (float) boost;

            Type t = typeof(T);

            var properties = t.GetProperties();
            
            string key = null;

            foreach(var property in properties)
            {
                var propValue = property.GetValue(doc);

                if (propValue != null)
                {
                    switch (property.Name)
                    {
                        case "id":
                            key = propValue.ToString();
                            luceneDoc.Add(new Field(property.Name, propValue.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                            break;

                        case "title":
                            luceneDoc.Add(new Field(property.Name, propValue.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                            break;

                        default:
                            luceneDoc.Add(new Field(property.Name, propValue.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                            break;
                    }
                }
            }

            if (key != null)
            {
                try
                {
                    Writer.UpdateDocument(new Term("id", key), luceneDoc);
                    //Writer.AddDocument(luceneDoc);
                }
                catch
                {
                    Writer.Dispose();
                }
            }

        }
       
       

        /// <summary>
        /// returns lucene index writer
        /// </summary>
        /// <returns></returns>
        private IndexWriter GetWriter()
        {
            var directory = FSDirectory.Open(SrchConnStr);
            var analyzer = new StandardAnalyzer(LuceneVer);
            
            
            return new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }


        /// <summary>
        /// Intialization after posting is completed
        /// </summary>
        public void PostCommit()
        {
           if(_Writer != null)
           {
               Writer.Optimize();
               Writer.Commit();
           }

        }

        public void Close()
        {
            if (_Writer != null)
            {
                Writer.Dispose();
            }
        }

        /// <summary>
        /// Intialization before posting begins
        /// </summary>
        public void PostInit()
        {
            
        }

        /*
        public List<string> GetSynonyms()
        {
            throw new NotImplementedException();
        }
         * */


        public void DeleteById(string id)
        {
            var term = new Term("id", id);
            Writer.DeleteDocuments(term);
            Writer.Optimize();
            Writer.Commit();
            Writer.Dispose();
        }


        public List<string> GetTerms(string fieldName, string term)
        {
            throw new NotImplementedException();
        }
        


        public void Reload()
        {
            throw new NotImplementedException();
        }


        public void Delete(string query)
        {
            throw new NotImplementedException();
        }


        public List<T> GetAll(string queryText)
        {
            throw new NotImplementedException();
        }


        public string Extract(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }
        
        SearchResponse<T> ISearchClient<T>.Search(string queryText)
        {
            throw new NotImplementedException();
        }

        List<T> ISearchClient<T>.GetAll(string queryText)
        {
            throw new NotImplementedException();
        }

        string ISearchClient<T>.FileExtract(byte[] fileBytes)
        {
            throw new NotImplementedException();
        }

        void ISearchClient<T>.Post(T doc)
        {
            throw new NotImplementedException();
        }

        void ISearchClient<T>.DeleteById(string id)
        {
            throw new NotImplementedException();
        }
        
        List<string> ISearchClient<T>.GetTerms(string fieldName, string term)
        {
            throw new NotImplementedException();
        }

        void ISearchClient<T>.Commit()
        {
            throw new NotImplementedException();
        }

        void ISearchClient<T>.Close()
        {
            throw new NotImplementedException();
        }

        void ISearchClient<T>.Reload()
        {
            throw new NotImplementedException();
        }


        public void Post(string jsonDoc)
        {
            throw new NotImplementedException();
        }


        SearchResponse ISearchClient.Search(SearchRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
