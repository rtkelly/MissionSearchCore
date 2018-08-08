using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MissionSearch.Clients
{
    internal static class SolrQueryBuilder 
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string BuildSearchQuery<T>(SearchRequest request) where T : ISearchDocument 
        {
            var solrQueryString = new StringBuilder();
             
            return solrQueryString
                .Append(string.Format("?q={0}&wt=json", request.QueryText))
                .AppendFields<T>(request)
                .AppendSort(request)
                .AppendHighlighting(request)
                .AppendFacets(request)
                .AppendQueryOptions(request.QueryOptions)
                .AppendRefinementFilters(request)
                .Append(string.Format("&rows={0}&start={1}", request.PageSize, request.Start))
                .ToString();
        }

        public static string BuildSearchQuery(SearchRequest request)
        {
            var solrQueryString = new StringBuilder();

            return solrQueryString
                .Append(string.Format("?q={0}&wt=json", request.QueryText))
                .AppendSort(request)
                .AppendHighlighting(request)
                .AppendFacets(request)
                .AppendQueryOptions(request.QueryOptions)
                .AppendRefinementFilters(request)
                .Append(string.Format("&rows={0}&start={1}", request.PageSize, request.Start))
                .ToString();
        }


        private static StringBuilder AppendFields<T>(this StringBuilder query, SearchRequest request) where T : ISearchDocument 
        {
            var docProps = typeof(T).GetProperties();

            var props = new List<string>();

            foreach(var prop in docProps)
            {
                if (prop.Name == "highlightsummary" && request.EnableHighlighting == false)
                    continue;

                props.Add(prop.Name);
            }
            
            var str = string.Format("&fl={0}", string.Join(",", props));

            return query.Append(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static StringBuilder AppendSort(this StringBuilder query, SearchRequest request)
        {
            var sortOptions = new List<string>();

            if (request.Sort == null || !request.Sort.Any())
                return query;

            foreach (var sortOption in request.Sort)
            {
                sortOptions.Add(string.Format("{0}+{1}", sortOption.SortField,
                    sortOption.Order == SortOrder.SortOption.Ascending ? "asc" : "desc"));
            }

            return query.Append(string.Format("&sort={0}", string.Join(",", sortOptions)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static StringBuilder AppendHighlighting(this StringBuilder query, SearchRequest request)
        {
            return query.Append(request.EnableHighlighting ? "&hl.fl=highlightsummary&hl=on" : "");
        }

        private static StringBuilder AppendRefinementFilters(this StringBuilder query, SearchRequest request)
        {
            var queryOptions = QueryOptions.ParseRefinementString(request.Refinements);

            var str = new StringBuilder();

            if (queryOptions == null || !queryOptions.Any())
                return query;

            var filterEqualQueries = queryOptions
                            //.OfType<FilterQuery>()
                            .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Equals)
                            .Select(qp => string.Format("&fq={0}:{1}", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();

            if (filterEqualQueries.Any())
                str.Append(string.Join("", filterEqualQueries));

            return query.Append(str.ToString());

        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryOptions"></param>
        /// <returns></returns>
        private static StringBuilder AppendQueryOptions(this StringBuilder query, List<IQueryOption> queryOptions)
        {
            var str = new StringBuilder();

            if (queryOptions == null || !queryOptions.Any())
                return query;
                        
            var filterEqualQueries = queryOptions
                             .OfType<FilterQuery>()
                             .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Equals)
                             .Select(qp => string.Format("&fq={0}:{1}", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();
            
            var filterGreaterQueries = queryOptions
                             .OfType<DateFilterQuery>()
                             .Where(fq => fq.Condition == DateFilterQuery.ConditionalTypes.GreaterThenEqual)
                             .Select(qp => string.Format("&fq={0}:[{1} TO *]", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();
            
            var filterLessQueries = queryOptions
                           .OfType<DateFilterQuery>()
                           .Where(fq => fq.Condition == DateFilterQuery.ConditionalTypes.LessThenEqual)
                           .Select(qp => string.Format("&fq={0}:[* TO {1}]", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();

            var filterDateRangeQueries = queryOptions
                           .OfType<RangeQuery<DateTime>>()
                           .Select(qp => string.Format("&fq={0}:[{1} TO {2}]", qp.ParameterName, 
                           FormatToString(qp.GreaterThenValue), FormatToString(qp.LessThenValue))).ToList();
            
            var filterWildcardQueries = queryOptions
                           .OfType<FilterQuery>()
                           .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Contains)
                           .Select(qp => string.Format("&fq={0}:{1}", qp.ParameterName, FormatWildCard(qp.FieldValue))).ToList();
            
            var filterNotQueries = queryOptions
                           .OfType<FilterQuery>()
                           .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.NotEqual)
                           .Select(qp => string.Format("&fq=-{0}:{1}", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();

            var disMaxQueryParms = queryOptions
                           .OfType<DisMaxQueryParm>()
                           .Select(qp => ProcessQueryOption(qp.ParameterName, qp.ParameterValue)).ToList();

            var queryParms = queryOptions
                           .OfType<QueryParm>()
                           .Select(qp => ProcessQueryOption(qp.ParameterName, qp.ParameterValue)).ToList();


            if (queryParms.Any())
                str.Append(string.Join("", queryParms));

            if (disMaxQueryParms.Any())
                str.Append(string.Join("", disMaxQueryParms));

            if (filterEqualQueries.Any())
                str.Append(string.Join("", filterEqualQueries));

            if (filterGreaterQueries.Any())
                str.Append(string.Join("", filterGreaterQueries));

            if (filterLessQueries.Any())
                str.Append(string.Join("", filterLessQueries));

            if(filterDateRangeQueries.Any())
                str.Append(string.Join("", filterDateRangeQueries));

            if (filterWildcardQueries.Any())
                str.Append(string.Join("", filterWildcardQueries));

            if (filterNotQueries.Any())
                str.Append(string.Join("", filterNotQueries));
                        
            var boostQueries = queryOptions
                               .OfType<BoostQuery>()
                               .Select(qp => string.Format("&bq={0}:{1}^{2}", qp.ParameterName, qp.FieldValue, qp.Boost)).ToList();

            if (boostQueries.Any())
                str.Append(string.Join("", boostQueries));

            if(boostQueries.Any() || disMaxQueryParms.Any())
            {
                str.Append("&defType=dismax");
            }

            return query.Append(str.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parmName"></param>
        /// <param name="parmValue"></param>
        /// <returns></returns>
        private static string ProcessQueryOption(string parmName, string parmValue)
        {
            switch(parmName)
            {
                case "pf":

                    if (Regex.IsMatch(parmValue, @"^-?[0-9]\d*(\.\d+)?$"))
                    {
                        return string.Format("&pf=_text_^{0}", parmValue);
                    }
                    else
                    {
                        return string.Format("&pf={0}", parmValue);
                    }

                case "mm":
                case "ps":

                    if (Regex.IsMatch(parmValue, @"^-?[0-9]\d*(\.\d+)?$"))
                    {
                        return string.Format("&{0}={1}", parmName, parmValue);
                    }

                    return "";

                default:
                    return string.Format("&{0}={1}", parmName, parmValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string FormatToString(object value)
        {
            if (value == null)
                return "";

            if(value is DateTime)
            {
                return string.Format("{0}", ((DateTime) value).ToString("yyyy-MM-ddThh:mm:ssZ"));
            }
            else if(value is string)
            {
                var str = value.ToString().Trim();

                // wrap in quotes if multi-word and not range
                if((str.Contains(" ") || str.Contains("/")) && !str.StartsWith("[") && !str.Contains("\""))
                {
                    string prop;

                    if(str.Contains(" OR "))
                    {
                        //str = Regex.Replace(str, " or ", " OR ", RegexOptions.IgnoreCase);
                        var values = str.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        prop = string.Join(" OR ", values.Select(v => FormatToString(v.Trim())));
                        prop = string.Format("({0})", prop);
                    }
                    else if (str.Contains(" AND "))
                    {
                        //str = Regex.Replace(str, " and ", " AND ", RegexOptions.IgnoreCase);
                        var values = str.Split(new[] { " AND " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        prop = string.Join(" AND ", values.Select(v => FormatToString(v.Trim())));
                        prop = string.Format("({0})", prop);
                    }

                    else {

                        prop = string.Format("\"{0}\"", HttpUtility.UrlEncode(str));
                    }
                    
                    return prop;
                }

            }
            
            return HttpUtility.UrlEncode(value.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string FormatWildCard(object o)
        {
            
            if (!(o is string))
                return "";

            var str = o.ToString(); 
            
            var terms = str.Split(' ');

            var wildCardQuery = new StringBuilder();
            
            var firstTerm = terms.First();

            foreach(var term in terms)
            {
                if (term == firstTerm)
                  wildCardQuery.Append(string.Format("*{0}*", term));
                else
                  wildCardQuery.Append(string.Format(" AND *{0}*", term));
            }
            
            return HttpUtility.UrlEncode(string.Format("({0})", wildCardQuery));
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static StringBuilder AppendFacets(this StringBuilder query, SearchRequest request)
        {
            var facets = new List<string>();

            var pivotFacets = request.Facets.OfType<PivotFacet>().ToList();

            foreach (var facet in pivotFacets)
            {
                facets.Add(string.Format("&facet.pivot={0}&facet.limit={1}&facet.pivot.mincount=1", facet.FieldName, facet.facetLimit));
                
            }
            
            var fieldFacets = request.Facets.OfType<FieldFacet>().ToList();
            
            foreach(var facet in fieldFacets)
            {
                facets.Add(string.Format("&facet.field={0}", facet.FieldName));
                //facets.Add(string.Format("&json.facet={{x:'hll({0})'}}", facet.FieldName)); 
                //facets.Add(string.Format("&json.facet={{categories: {{ type : terms, field : {0}, facet : {{ x : \"hll({0})\" }} }}}}", facet.FieldName)); 
            }

            var categoryFacets = request.Facets.OfType<CategoryFacet>().ToList();

            foreach (var facet in categoryFacets)
            {
                facets.Add(string.Format("&facet.field={0}", facet.FieldName));
            }

            var rangeFacets = request.Facets.OfType<NumRangeFacet>();

            foreach (var facet in rangeFacets)
            {
                foreach (var gap in facet.Range)
                {
                    var lower = gap.Lower == null ? "*" : gap.Lower.Value.ToString(CultureInfo.InvariantCulture);
                    var upper = gap.Upper == null ? "*" : gap.Upper.Value.ToString(CultureInfo.InvariantCulture);
                    
                    facets.Add(string.Format("&facet.query={0}:[{1}+TO+{2}]", facet.FieldName, lower, upper)); 
                }
            }
            
            var dateFacets = request.Facets.OfType<DateRangeFacet>().ToList();

            foreach (var facet in dateFacets)
            {
                foreach (var gap in facet.Ranges)
                {
                    var lower = gap.Lower == null ? "*" : gap.Lower.Value.Date.ToString("yyyy-MM-ddThh:mm:ssZ");
                    var upper = gap.Upper == null ? "*" : gap.Upper.Value.Date.ToString("yyyy-MM-ddThh:mm:ssZ");

                    facets.Add(string.Format("&facet.query={0}:[{1}+TO+{2}]", facet.FieldName, lower, upper));
                }

               
            }
            
            if (facets.Any())
            {
              facets.Add("&facet=on&facet.mincount=0");
            }

            return query.Append(string.Join("", facets));
        }

        /*
        private static string AppendLanguage(SearchRequest request)
        {
            if(string.IsNullOrEmpty(request.Language))
                return "";

            return string.Format("&fq=language:{0}", request.Language);
        }
        */

    }
}
