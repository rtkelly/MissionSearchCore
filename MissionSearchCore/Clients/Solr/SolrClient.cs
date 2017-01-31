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

namespace MissionSearch.Clients
{
    public class SolrClient<T> : ISearchClient<T> where T : ISearchDocument 
    {
        string _srchConnStr;
                
        public string SrchConnStr { get { return _srchConnStr; } }

        public int Timeout { get; set; }

        public ILogger _logger { get; set; }

        string EndPointAdd  { get { return string.Format("{0}/update", SrchConnStr); } }
        string EndPointCommit { get { return string.Format("{0}/update?commit=true", SrchConnStr); } }
        //string EndPointExtract { get { return string.Format("{0}/update/extract", SrchConnStr); } }
        string EndPointDeleteById { get { return string.Format("{0}/update?commit=true&stream.body={1}", SrchConnStr, "<delete><query>id:{0}</query></delete>"); } }
        string EndPointDelete { get { return string.Format("{0}/update?commit=true&stream.body={1}", SrchConnStr, "<delete><query>{0}</query></delete>"); } }
        string EndPointSearch { get { return string.Format("{0}/select", SrchConnStr); } }
        string EndPointExtractOnly { get { return string.Format("{0}/update/extract?&extractOnly=true", SrchConnStr); } }
        //string EndPointGetSynonyms { get { return string.Format("{0}/schema/analysis/synonyms/english", SrchConnStr); } }
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchConnectionString"></param>
        public SolrClient(string srchConnectionString)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Solr Core undefined");

            _srchConnStr = srchConnectionString;

            _logger = SearchFactory<T>.Logger;
                        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srchConnectionString"></param>
        /// <param name="logger"></param>
        public SolrClient(string srchConnectionString, ILogger logger)
        {
            if (string.IsNullOrEmpty(srchConnectionString))
                throw new NotImplementedException("Solr Core undefined");

            _srchConnStr = srchConnectionString;

            _logger = logger;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void Post(T doc)
        {
            var settings = new JsonSerializerSettings();
            settings.DateFormatString = "yyyy-MM-ddTHH:mm:ssZ";
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

            var jsonDoc = JsonConvert.SerializeObject(CleanDoc(doc), settings);

            Post(jsonDoc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonDoc"></param>
        public void Post(string jsonDoc)
        {
            if (jsonDoc == null)
                return;

            //jsonDoc = AppendToDoc(jsonDoc, doc);
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
       /// <param name="doc"></param>
       /// <param name="fileBytes"></param>
       /// <returns></returns>
        public T Extract(T doc, byte[] fileBytes)
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
                    return doc;

                using (var rdr = new StreamReader(webStream))
                {
                    var responseXml = rdr.ReadToEnd();
                    var xmlParser = new XmlParser(responseXml);
                    var xhtml = xmlParser.ParseHTML("/response/str");
                    var htmlParser = new HtmlParser(WebUtility.HtmlDecode(xhtml));

                    //doc.mimetype = xmlParser.ParseString("/response/lst/arr[@name='Content-Type']/str");
                    //var pubdate = xmlParser.ParseDate("/response/lst/arr[@name='Creation-Date']/str");

                    //if (pubdate != null)
                     //   doc.timestamp = pubdate.Value;

                    if (doc.content == null)
                        doc.content = new List<string>();

                    doc.content.Add(WebUtility.HtmlEncode(htmlParser.ParseStripInnerHtml("//body")));
                }
                

                return doc;
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
                if (string.IsNullOrEmpty(request.QueryText))
                    request.QueryText = "*:*";

                // TO DO: replace with regex
                //Regex.Replace("")
                request.QueryText = request.QueryText.Replace(">", "");
                request.QueryText = request.QueryText.Replace("<", "");
                request.QueryText = request.QueryText.Replace("#", "");
                request.QueryText = request.QueryText.Replace("^", "");
                request.QueryText = request.QueryText.Replace("&", "");
                request.QueryText = request.QueryText.Replace("?", "");
                request.QueryText = request.QueryText.Replace(")", "");
                request.QueryText = request.QueryText.Replace("(", "");
                request.QueryText = request.QueryText.Replace("]", "");
                request.QueryText = request.QueryText.Replace("[", "");
                request.QueryText = request.QueryText.Replace("{", "");
                request.QueryText = request.QueryText.Replace("}", "");
                request.QueryText = request.QueryText.Replace("~", "");
                request.QueryText = request.QueryText.Replace("`", "");

                if (request.QueryText.Count(x => x == '"') == 1)
                    request.QueryText = request.QueryText.Replace("\"", "");

                if (!string.IsNullOrEmpty(request.Refinements))
                {
                    var refinementList = QueryOptions.ParseRefinementString(request.Refinements);

                    if (refinementList.Any())
                        request.QueryOptions.AddRange(refinementList);
                }
                                
                var queryText=string.Format("{0}{1}", EndPointSearch, SolrQueryBuilder.BuildSearchQuery(request));
                srchResponse.QueryString = queryText;

                //if (request.EnableQueryLogging)
                //{
                    if (_logger != null)
                        _logger.Info(srchResponse.QueryString);
                //}

                var httpRequest = (HttpWebRequest)WebRequest.Create(srchResponse.QueryString);

                if(Timeout > 0) httpRequest.Timeout = Timeout;

                using (var webResponse = (HttpWebResponse)httpRequest.GetResponse())
                {
                    var webStream = webResponse.GetResponseStream();

                    if (webStream == null)
                        return srchResponse;

                    using (var rdr = new StreamReader(webStream))
                    {
                        srchResponse.JsonResponse = rdr.ReadToEnd();
                    }

                    var responseContainer = JsonConvert.DeserializeObject<SolrResponseContainer<T>>(srchResponse.JsonResponse, new JsonSerializerSettings()
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                        });

                    srchResponse.TotalFound = responseContainer.response.numFound;
                    srchResponse.Results = responseContainer.response.docs;
                    srchResponse.PageSize = request.PageSize;
                    srchResponse.CurrentPage = request.CurrentPage;

                    if (responseContainer.highlighting != null)
                    {
                        foreach (var doc in srchResponse.Results)
                        {
                            if (responseContainer.highlighting.ContainsKey(doc.id))
                            {
                                var highdoc = responseContainer.highlighting[doc.id].FirstOrDefault();

                                if (highdoc.Value != null && highdoc.Value.Any())
                                {
                                    doc.highlightsummary = highdoc.Value.First();

                                    //if(doc.highlightsummary.StartsWith(","))
                                    //{
                                    //   doc.highlightsummary = doc.highlightsummary.Substring(1, doc.highlightsummary.Length - 1).Trim();
                                    //}

                                }
                                else
                                    doc.highlightsummary = doc.summary ?? "";
                            }
                        }
                    }

                    if (responseContainer.facet_counts != null)
                        srchResponse.Refinements = LoadRefinements(responseContainer.facet_counts, request);

                    //if(request.QueryIndexer != null && !string.IsNullOrEmpty(request.QueryText) && !Regex.IsMatch(request.QueryText, @"\*"))
                    //{
                    // request.QueryIndexer.AddUpdateTerm(request.QueryText, request.Language);
                    //}
                    srchResponse.Success = true;
                    return srchResponse;
                }

            }
            catch(Exception ex)
            {
                srchResponse.ErrorMessage = ex.Message;
                
                return srchResponse;
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

        public void Delete(string query)
        {
            try
            {
                var url = string.Format(EndPointDelete, query);
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
        public void PostCommit()
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPointCommit);
            request.GetResponse();   
        }


        /// <summary>
        /// Intialization proccess required before processing posts
        /// </summary>
        public void PostInit()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetSynonyms()
        {
            throw new NotImplementedException();
        }


        
        /// <summary>
        /// Load Solr Facet counts into list of refinement objects
        /// </summary>
        /// <param name="facetResponse"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<Refinement> LoadRefinements(SolrFacetCounts facetResponse, SearchRequest request)
        {
            var refinements = new List<Refinement>();
            var currentFilterQueries = request.QueryOptions.OfType<FilterQuery>().ToList();

            foreach (var facetField in request.Facets)
            {
                var refinement = refinements.FirstOrDefault(r => r.Name == facetField.FieldName);

                if (refinement == null)
                {
                    refinement = new Refinement();
                    refinement.Name = facetField.FieldName;
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
                            Label = item,
                            Value = string.Format("\"{0}\"", item),
                            Refinement = queryParm.FieldValue.ToString(),
                            Count = TypeParser.ParseLong(itemCounts[index]),
                            //Selected = currentFilterQueries.Any(f => f.FieldValue.ToString().Replace("\"", "") == item),
                            Selected = currentFilterQueries.Any(f => f.FieldValue.ToString().Contains(string.Format("\"{0}\"", item))),
                        };
                        
                        //facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, fieldFacet.RefinementOption));
                        facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, request.RefinementType));                        
                        refinement.Items.Add(facetItem);
                     }
                    
                    if(fieldFacet.Sort == FacetSortOption.Name )
                    {
                        refinement.Items = refinement.Items.OrderBy(i => i.Label).ToList();
                    }
                }

                
            }

            var rangeFacets = request.Facets.OfType<NumRangeFacet>().ToList();
            var dateFacets = request.Facets.OfType<DateRangeFacet>().ToList();
                        
            // load facet query fields
            foreach (var query in facetResponse.facet_queries)
            {
                if (query.Value == 0)
                    continue;

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
                        Label = string.Format("{0}{1}", start.ToString(format),  Math.Abs(end) < 0.0 ? " or more " : " - " + end.ToString(format)),
                        Value = propValue,
                        Refinement = queryParm.FieldValue.ToString(),
                        Count = query.Value,
                        Selected = currentFilterQueries.Any(f => f.FieldValue.ToString() == propValue)
                    };

                    facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, request.RefinementType));

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
                        Label = label,
                        Value = propValue,
                        Refinement = queryParm.FieldValue.ToString(),
                        Count = query.Value,
                        Selected = currentFilterQueries.ToList().Any(f => f.FieldValue.ToString() == propValue),
                    };

                    facetItem.Link = string.Format("&ref={0}", AddRemoveRefinement(facetItem, request.Refinements, request.RefinementType));

                    if (refinement != null) refinement.Items.Add(facetItem);
                }
            }
                                    

            return refinements
                .Where(r => r.Items.Any())
                //.OrderBy(r => r.Order)
                .ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentRefinementsStr"></param>
        /// <param name="refinement"></param>
        /// <param name="refinementType"></param>
        /// <returns></returns>
        public string AddRemoveRefinement(RefinementItem refinement, string currentRefinementsStr, RefinementTypes refinementType)
        {
            var refinementStr = string.Format("{0};{1}", refinement.Name, refinement.Value);
                        
            if (string.IsNullOrEmpty(currentRefinementsStr))
            {
                return StringEncoder.EncodeString(refinementStr);
            }

            var decodedCurrentRefinements = StringEncoder.DecodeString(currentRefinementsStr) ?? "";
            
            var currentRefinements = decodedCurrentRefinements.Split(',').ToList();
            
            var currentRefinementItems = currentRefinements.Select(r => new RefinementItem(r)).ToList();
           
            switch (refinementType)
            {
                case RefinementTypes.SingleSelect:

                    if (decodedCurrentRefinements.Contains(refinementStr))
                    {
                        return StringEncoder.EncodeString(string.Join(",", currentRefinements.Where(p => p != refinementStr)));
                    }
                    else if (decodedCurrentRefinements.Contains(refinement.Name))
                    {
                        decodedCurrentRefinements = string.Join(",", currentRefinements.Where(p => !p.Contains(refinement.Name)));
                        
                        return StringEncoder.EncodeString(string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
                    }

                    break;

                case RefinementTypes.MultiSelect:

                    var likeRefinement = currentRefinementItems.FirstOrDefault(p => p.Name == refinement.Name);
                    
                    if(likeRefinement != null)
                    {
                        var rawValues = likeRefinement.Value.Replace("(", "").Replace(")", "");

                        var values = Regex.Split(rawValues, " OR ");

                        var valueStr = string.Join(" OR ", values.Where(v => v != refinement.Value));

                        if (values.Any(v => v == refinement.Value))
                        {
                            refinementStr = (string.IsNullOrEmpty(valueStr)) ? "" : string.Format("{0};({1})", refinement.Name, valueStr);
                        }
                        else
                        {
                            refinementStr = string.Format("{0};({1} OR {2})", refinement.Name, valueStr, refinement.Value);
                        }
                    }

                    return StringEncoder.EncodeString(refinementStr);
                                       
                  
                default:
                        
                    if (decodedCurrentRefinements.Contains(refinementStr))
                    {
                        return StringEncoder.EncodeString(string.Join(",", currentRefinements.Where(p => p != refinementStr)));
                    }
                    
                    break;
            }

            return StringEncoder.EncodeString(string.Format("{0},{1}", decodedCurrentRefinements, refinementStr));
            
           

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
        public List<T> SearchAll(string q)
        {
            var indexedPages = new List<T>();

            var response = Search(new SearchRequest()
            {
                QueryText = q,
                PageSize = 500,
            });

            if (response.Results.Any())
                indexedPages.AddRange(response.Results);

            for (int page = 2; page <= response.TotalPages; page++)
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