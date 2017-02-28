using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using MissionSearch.Util;
using System.Text.RegularExpressions;
using System.Web;
using MissionSearch.Search.Facets;
using MissionSearch.Search.Refinements;

namespace MissionSearch.Clients
{
    public class SolrClient : ISearchClient
    {
        string _srchConnStr;
        public string SrchConnStr { get { return _srchConnStr; } }

        public int Timeout { get; set; }

        protected string EndPointAdd { get { return string.Format("{0}/update", SrchConnStr); } }
        protected string EndPointCommit { get { return string.Format("{0}/update?commit=true", SrchConnStr); } }
        protected string EndPointDeleteById { get { return string.Format("{0}/update?commit=true&stream.body={1}", SrchConnStr, "<delete><query>id:{0}</query></delete>"); } }
        protected string EndPointDelete { get { return string.Format("{0}/update?commit=true&stream.body={1}", SrchConnStr, "<delete><query>{0}</query></delete>"); } }
        protected string EndPointSearch { get { return string.Format("{0}/select", SrchConnStr); } }
        protected string EndPointExtractOnly { get { return string.Format("{0}/update/extract?&extractOnly=true", SrchConnStr); } }
        //string EndPointGetSynonyms { get { return string.Format("{0}/schema/analysis/synonyms/english", SrchConnStr); } }
        //string EndPointExtract { get { return string.Format("{0}/update/extract", SrchConnStr); } }
        
        public SolrClient(string srchConnectionString)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Solr Core undefined");

            _srchConnStr = srchConnectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonDoc"></param>
        public void Post(string jsonDoc)
        {
            if (jsonDoc == null)
                return;

            var bytes = Encoding.UTF8.GetBytes(string.Format("[{0}]", jsonDoc));
            var request = (HttpWebRequest)WebRequest.Create(EndPointAdd);
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                var resp = (HttpWebResponse)request.GetResponse();

                resp.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public void Delete(string query)
        {
            var url = string.Format(EndPointDelete, query);
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.GetResponse();
        }

        public SearchResponse Search(string queryText)
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
        public SearchResponse Search(SearchRequest request)
        {
            var srchResponse = new SearchResponse();
            
            if (string.IsNullOrEmpty(request.QueryText))
                request.QueryText = "*:*";

            srchResponse.QueryString = string.Format("{0}{1}", EndPointSearch, SolrQueryBuilder.BuildSearchQuery(request));

            var httpRequest = (HttpWebRequest)WebRequest.Create(srchResponse.QueryString);
                        
            using (var webResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                var webStream = webResponse.GetResponseStream();

                if (webStream == null)
                    return srchResponse;

                using (var rdr = new StreamReader(webStream))
                {
                    srchResponse.JsonResponse = rdr.ReadToEnd();
                }

                srchResponse.ResponseContainer = JsonConvert.DeserializeObject<SolrResponseContainer>(srchResponse.JsonResponse, new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                });

                if (srchResponse.ResponseContainer != null)
                {
                    srchResponse.Results = srchResponse.ResponseContainer.response.docs;
                }
            }

            srchResponse.QueryString = request.QueryText;

            return srchResponse;
        }
    }
    
    public class SolrClient<T> : SolrClient, ISearchClient<T> where T : ISearchDocument 
    {
        
        public SolrClient(string srchConnectionString) : base(srchConnectionString)
        {
          
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void Post(T doc)
        {
            //1995-12-31T23:59:59Z
            var settings = new JsonSerializerSettings();
            settings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

            var jsonDoc = JsonConvert.SerializeObject(CleanDoc(doc), settings);

            Post(jsonDoc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private T CleanDoc(T doc)
        {
            var properties = doc.GetType().GetProperties();

            foreach(var property in properties)
            {
                switch (property.PropertyType.Name)
                {
                    case "String":
                    case "string":

                        var val = property.GetValue(doc);

                        if (val == null)
                            property.SetValue(doc, "");

                        break;
                }
            }

            return doc;
        }
       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        public string FileExtract(byte[] fileBytes)
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPointExtractOnly);

            request.Method = "Post";
            request.KeepAlive = true;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.ContentLength = fileBytes.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileBytes, 0, fileBytes.Length);
                requestStream.Close();

                var webResp = (HttpWebResponse)request.GetResponse();
                var webStream = webResp.GetResponseStream();

                if (webStream == null)
                    return string.Empty;

                using (var rdr = new StreamReader(webStream))
                {
                    var responseXml = rdr.ReadToEnd();
                    return responseXml;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public new SearchResponse<T> Search(string queryText)
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
            var srchResponse = BaseSearch(request);

            try
            {
                var responseContainer = JsonConvert.DeserializeObject<SolrResponseContainer<T>>(srchResponse.JsonResponse, new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                });

                srchResponse.TotalFound = responseContainer.response.numFound;
                srchResponse.Results = responseContainer.response.docs;

                foreach (var doc in srchResponse.Results)
                {
                    if (responseContainer.highlighting != null)
                    {
                        if (responseContainer.highlighting.ContainsKey(doc.id))
                        {
                            var highdoc = responseContainer.highlighting[doc.id].FirstOrDefault();

                            if (highdoc.Value != null && highdoc.Value.Any())
                            {
                                doc.highlightsummary = highdoc.Value.First().Replace("<em>", "<strong>").Replace("</em>", "</strong>").Replace("\"", "");
                            }

                        }
                    }
                }
                
                if (responseContainer.facet_counts != null)
                    srchResponse.Refinements = LoadRefinements(responseContainer.facet_counts, request);

                //if(request.QueryIndexer != null && !string.IsNullOrEmpty(request.QueryText) && !Regex.IsMatch(request.QueryText, @"\*"))
                //{
                // request.QueryIndexer.AddUpdateTerm(request.QueryText, request.Language);
                //}

                if (request.Facets.OfType<IFacet>().Any(f => f.RefinementOption == RefinementType.MultiSelect || f.RefinementOption == RefinementType.SingleSelect))
                {
                    UpdateFacetCounts(request, srchResponse);
                }

                srchResponse.Refinements.ForEach(r => r.Items = r.Items.Where(i => i.Count > 0 || i.Selected == true).ToList());
                srchResponse.Refinements.RemoveAll(r => r.Items.Count == 0);
            
                foreach (var facet in request.Facets)
                {
                    var first = srchResponse.Refinements.FirstOrDefault(r => r.Label == facet.FieldLabel);

                    if (first != null)
                    {
                        first.Items = SortRefinementItems(first, facet.Sort);
                    }
                }
                
                srchResponse.Success = true;
                return srchResponse;
            }
            catch
            {
                throw new Exception("error in query " + srchResponse.QueryString);
                
            }
        }

        private SearchResponse<T> BaseSearch(SearchRequest request)
        {
            var srchResponse = new SearchResponse<T>();
            srchResponse.Results = new List<T>();
           
            if (string.IsNullOrEmpty(request.QueryText))
                request.QueryText = "*:*";
                       
            request.QueryText = Regex.Replace(request.QueryText, @"[>|<|#|^|&|?|\)|\(|\]|\[|\}|\{|~|`]", "");
           
            if (request.QueryText.Count(x => x == '"') == 1)
                request.QueryText = request.QueryText.Replace("\"", "");

            var queryText = string.Format("{0}{1}", EndPointSearch, SolrQueryBuilder.BuildSearchQuery<T>(request));
            srchResponse.QueryString = queryText;

            if (request.EnableQueryLogging)
            {
                var logger = SearchFactory.Logger;

                if (logger != null)
                    logger.Info(srchResponse.QueryString);
            }

            var httpRequest = (HttpWebRequest)WebRequest.Create(srchResponse.QueryString);

            using (var webResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                var webStream = webResponse.GetResponseStream();

                if (webStream == null)
                    return srchResponse;

                using (var rdr = new StreamReader(webStream))
                {
                    srchResponse.JsonResponse = rdr.ReadToEnd();
                }
                   
                srchResponse.PageSize = request.PageSize;
                srchResponse.CurrentPage = request.CurrentPage;
                srchResponse.Success = true;

                return srchResponse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="srchResponse"></param>
        private void UpdateFacetCounts(SearchRequest request, SearchResponse<T> srchResponse)
        {
            if (string.IsNullOrEmpty(request.Refinements))
                return;

            var facets = request.Facets.OfType<IFacet>().Where(f => f.RefinementOption == RefinementType.MultiSelect
                    || f.RefinementOption == RefinementType.SingleSelect).ToList();

            var decodedCurrentRefinements = StringEncoder.DecodeString(request.Refinements) ?? "";
            var currentRefinements = decodedCurrentRefinements.Split(',').ToList();
            
            foreach(var facet in facets)
            { 
                var request2 = new SearchRequest()
                {
                    PageSize = 1,
                    QueryOptions = request.QueryOptions,
                    QueryText = request.QueryText,
                    Facets = new List<IFacet>() { facet },
                    Refinements = StringEncoder.EncodeString(string.Join(",", currentRefinements.Where(p => !p.Contains(facet.FieldLabel)))),
                };

                var response2 = BaseSearch(request2);

                var responseContianer = response2.ResponseContianer;

                if (responseContianer.facet_counts == null)
                    continue;
                
                response2.Refinements = LoadRefinements(responseContianer.facet_counts, request2);

                var refinement = response2.Refinements.FirstOrDefault();

                if (refinement == null)
                    continue;
                    
                var originalRefinement = srchResponse.Refinements.FirstOrDefault(f => f.Name == refinement.Name);

                if (originalRefinement == null)
                    continue;

                foreach (var item in refinement.Items)
                {
                    var originalItem = originalRefinement.Items.FirstOrDefault(i => i.Value == item.Value);

                    if (originalItem != null)
                    {
                        originalItem.Count = item.Count;
                    }
                }
            }
        }

        private List<RefinementItem> SortRefinementItems(Refinement refinement, FacetSortOption sortOption)
        {
            if (sortOption == FacetSortOption.Name)
            {
                return refinement.Items.OrderBy(p => p.DisplayName).ToList();
            }
            else
            {
                return refinement.Items.OrderByDescending(p => p.Count).ThenBy(p => p.DisplayName).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void DeleteById(string id)
        {
            try
            {
                var url = string.Format(EndPointDeleteById, id);
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.GetResponse();
            }
            catch(Exception ex)
            {
                // TO DO: handle exception
            }

        }

        


        /// <summary>
        /// Cleanup proccess for after processing posts
        /// </summary>
        public void Commit()
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPointCommit);
            request.GetResponse();   
        }


        /// <summary>
        /// Intialization proccess required before processing posts
        /// </summary>
        /// 
        /*
        public void PostInit()
        {
            
        }
         * */

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /*
        public List<string> GetSynonyms()
        {
            throw new NotImplementedException();
        }
        */

        
        /// <summary>
        /// Load Solr Facet counts into list of refinement objects
        /// </summary>
        /// <param name="facetResponse"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<Refinement> LoadRefinements(SolrFacetCounts facetResponse, SearchRequest request)
        {
            var refinements = new List<Refinement>();
            var currentFilterQueries = QueryOptions.ParseRefinementString(request.Refinements);

            foreach (var facetField in request.Facets)
            {
                var refinement = refinements.FirstOrDefault(r => r.Name == facetField.FieldName);

                if (refinement == null || facetField is CategoryFacet)
                {
                    refinement = new Refinement();
                    refinement.Name = (facetField is CategoryFacet) ? ((CategoryFacet)facetField).CategoryName : facetField.FieldName;
                    refinement.Label = facetField.FieldLabel;
                    refinements.Add(refinement);
                    
                }
            }
                    
            // load field facets 
            foreach (var fieldFacet in request.Facets.OfType<FieldFacet>())
            {
                if(facetResponse.facet_fields.ContainsKey(fieldFacet.FieldName))
                {
                    var refinement = refinements.FirstOrDefault(r => r.Name == fieldFacet.FieldName);
                                   
                    if(refinement == null)
                        continue;

                    var items = facetResponse.facet_fields[refinement.Name];

                    var itemValues = items.Where((elem, idx) => idx % 2 == 0).ToList();
                    var itemCounts = items.Where((elem, idx) => idx % 2 != 0).ToList();

                    foreach(var item in itemValues)
                    {
                        var queryParm = new FilterQuery(refinement.Name, item);
                        var index = itemValues.IndexOf(item);

                        var facetItem = new RefinementItem()
                        {
                            Name = refinement.Name,
                            GroupLabel = refinement.Label,
                            DisplayName = item,
                            Value = string.Format("\"{0}\"", item),
                            Count = TypeParser.ParseLong(itemCounts[index]),
                            Selected = currentFilterQueries.Any(f => f.FieldValue.ToString().Contains(string.Format("\"{0}\"", item))),
                        };

                        facetItem.Refinement = RefinementBuilder.AddRemoveRefinement(facetItem, request.Refinements, fieldFacet.RefinementOption);
                        facetItem.Link = string.Format("&ref={0}", facetItem.Refinement);

                        //facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, request.RefinementType));                        
                        refinement.Items.Add(facetItem);
                     }

                    refinement.Items = SortRefinementItems(refinement, fieldFacet.Sort);
                }

                
            }

            foreach (var categoryFacet in request.Facets.OfType<CategoryFacet>())
            {
                var refinement = refinements.FirstOrDefault(r => r.Name == categoryFacet.CategoryName);

                if (refinement == null)
                    continue;

                var categoryPath = categoryFacet.CategoryName + "/";

                var items = facetResponse.facet_fields[categoryFacet.FieldName];

                var itemValues = items.Where((elem, idx) => idx % 2 == 0).ToList();
                var itemCounts = items.Where((elem, idx) => idx % 2 != 0).ToList();

                foreach (var item in itemValues)
                {
                    if (item == categoryFacet.CategoryName)
                        continue;

                    var queryParm = new FilterQuery(refinement.Name, item);
                    var index = itemValues.IndexOf(item);

                    if (item == categoryFacet.CategoryName || item.Contains(categoryPath))
                    {
                        var facetItem = new RefinementItem()
                        {
                            Name = categoryFacet.FieldName,
                            GroupLabel = categoryFacet.FieldLabel,
                            DisplayName = item.Replace(categoryPath, "").Trim(),
                            Value = string.Format("\"{0}\"", item),
                            Count = TypeParser.ParseLong(itemCounts[index]),
                            Selected = currentFilterQueries.Any(f => f.FieldValue.ToString().Contains(string.Format("\"{0}\"", item))),
                        };

                        facetItem.Refinement = RefinementBuilder.AddRemoveRefinement(facetItem, request.Refinements, categoryFacet.RefinementOption);
                        facetItem.Link = string.Format("&ref={0}", facetItem.Refinement);
                        
                        refinement.Items.Add(facetItem);
                    }
                }

                refinement.Items = SortRefinementItems(refinement, categoryFacet.Sort);
            }

            var rangeFacets = request.Facets.OfType<NumRangeFacet>().ToList();
            var dateFacets = request.Facets.OfType<DateRangeFacet>().ToList();
                        
            // load facet query fields
            foreach (var query in facetResponse.facet_queries)
            {
                // TO DO: find better parsing solution
                var index = query.Key.IndexOf(':');
                var propName = query.Key.Substring(0, index);
                var propValue = query.Key.Substring(index+1, query.Key.Length-index-1);

                var queryParm = new FilterQuery(propName, propValue);

                var rangeFacet = rangeFacets.FirstOrDefault(f => f.FieldName == propName);
                var dateFacet = dateFacets.FirstOrDefault(f => f.FieldName == propName);

                if (rangeFacet != null)
                {
                    var match = Regex.Match(propValue, @"(\d*)\sTO\s(\d*)");

                    if (!match.Success)
                        continue;

                    var start = TypeParser.ParseDouble(match.Groups[1].ToString());
                    var end = TypeParser.ParseDouble(match.Groups[2].ToString());

                    var refinement = refinements.FirstOrDefault(r => r.Name == propName);
                                        
                    var format = "";
                    switch (rangeFacet.NumericFormat)
                    {
                        case NumRangeFacet.FormatType.Currency:
                            format = "C";
                            break;
                        case NumRangeFacet.FormatType.Percentage:
                            format = "P";
                            break;
                        case NumRangeFacet.FormatType.Decimal:
                            format = "N";
                            break;
                        case NumRangeFacet.FormatType.Numeric:
                            format = "N0";
                            break;
                    }

                    var facetItem = new RefinementItem()
                    {
                        Name = propName,
                        GroupLabel = refinement.Label,
                        DisplayName = string.Format("{0}{1}", start.ToString(format),  Math.Abs(end) < 0.0 ? " or more " : " - " + end.ToString(format)),
                        Value = propValue,
                        Count = query.Value,
                        Selected = currentFilterQueries.Any(f => f.FieldValue.ToString() == propValue)
                    };

                    facetItem.Refinement = RefinementBuilder.AddRemoveRefinement(facetItem, request.Refinements, rangeFacet.RefinementOption);
                    facetItem.Link = string.Format("&ref={0}", facetItem.Refinement);
                    //facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, request.RefinementType));

                    if (refinement != null) refinement.Items.Add(facetItem);
                }
                else if(dateFacet != null)
                {
                    var match = Regex.Match(propValue, @"\[(.*)\sTO\s(.*)\]");

                    if (!match.Success)
                        continue;

                    var start = TypeParser.ParseDateExact(match.Groups[1].ToString());
                    var end = TypeParser.ParseDateExact(match.Groups[2].ToString());
                    
                    var format = "yyyy";

                    var refinement = refinements.FirstOrDefault(r => r.Name == propName);

                    var label = "";

                    if (start == null && end != null)
                    {
                        label = string.Format("{0} or earlier", end.Value.ToString(format));
                    }
                    else if (start != null && end == null)
                    {
                        label = string.Format("{0} or later", start.Value.ToString(format));
                    }
                    else if (start != null)
                    {
                        var rangeInfo = dateFacet.Ranges.FirstOrDefault(r => r.Lower == start || (r.Lower != null && r.Lower.Value.Date == start));
                        
                        label = (rangeInfo != null) ? rangeInfo.Label : end.Value.ToString(format);
                    }
                    else
                    {
                        continue;
                    }

                    var facetItem = new RefinementItem()
                    {
                        Name = propName,
                        GroupLabel = refinement.Label,
                        DisplayName = label,
                        Value = propValue,
                        Count = query.Value,
                        Selected = currentFilterQueries.ToList().Any(f => f.FieldValue.ToString() == propValue),
                    };

                    facetItem.Refinement = RefinementBuilder.AddRemoveRefinement(facetItem, request.Refinements, dateFacet.RefinementOption);
                    facetItem.Link = string.Format("&ref={0}", facetItem.Refinement);
                    //facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, request.RefinementType));

                    if (refinement != null) refinement.Items.Add(facetItem);
                }
            }


            return refinements;
                //.Where(r => r.Items.Any())
                //.OrderBy(r => r.Order)
                //.ToList();
        }

                  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public List<string> GetTerms(string fieldName, string term)
        {
            var EndPointGetResources = string.Format("{0}/terms?wt=json&terms.fl={1}&terms.sort=index&terms.lower={2}", SrchConnStr, fieldName, term);

            var json = HttpClient.GetRequest(EndPointGetResources);

            var resourceContainer = JsonConvert.DeserializeObject<SolrTermsContainer>(json, new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,

            });

            return resourceContainer.terms[fieldName];

        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {
            var index = SrchConnStr.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var coreName = SrchConnStr.Substring(index, SrchConnStr.Length - index);
            var solrUrl = SrchConnStr.Substring(0, index);

            var EndPointGetResources = string.Format("{0}admin/cores?action=RELOAD&core={1}", solrUrl, coreName);

            HttpClient.CallWebRequest(EndPointGetResources);
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
       
       
    }

    
}