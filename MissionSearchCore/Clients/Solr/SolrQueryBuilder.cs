using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MissionSearch.Clients
{
    internal class SolrQueryBuilder
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string BuildSearchQuery(SearchRequest request)
        {
            var solrQueryString = new StringBuilder();

            solrQueryString.Append(string.Format("?q={0}&wt=json", request.QueryText));
            solrQueryString.Append(AppendSort(request));
            solrQueryString.Append(AppendFacets(request));
            solrQueryString.Append(AppendHighlighting(request));
            solrQueryString.Append(AppendQueryOptions(request));
            solrQueryString.Append(string.Format("&rows={0}&start={1}", request.PageSize, request.Start));
            
            //solrQueryString.Append(AppendLanguage(request));
            return solrQueryString.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string AppendSort(SearchRequest request)
        {
            var sortOptions = new List<string>();

            if (request.Sort == null || !request.Sort.Any())
                return "";

            foreach (var sortOption in request.Sort)
            {
                sortOptions.Add(string.Format("{0}+{1}", sortOption.SortField,
                    sortOption.Order == SortOrder.SortOption.Ascending ? "asc" : "desc"));
            }

            return string.Format("&sort={0}", string.Join(",", sortOptions));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string AppendHighlighting(SearchRequest request)
        {
            return (request.EnableHighlighting) ? "&hl.fl=highlightsummary&hl=on" : "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string AppendQueryOptions(SearchRequest request)
        {
            var queryOptions = new StringBuilder();

            if (request.QueryOptions == null || !request.QueryOptions.Any())
                return "";
                        
            var filterEqualQueries = request.QueryOptions
                             .OfType<FilterQuery>()
                             .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Equals)
                             .Select(qp => string.Format("&fq={0}:{1}", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();
            
            var filterGreaterQueries = request.QueryOptions
                             .OfType<DateFilterQuery>()
                             .Where(fq => fq.Condition == DateFilterQuery.ConditionalTypes.GreaterThenEqual)
                             .Select(qp => string.Format("&fq={0}:[{1} TO *]", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();
            
            var filterLessQueries = request.QueryOptions
                           .OfType<DateFilterQuery>()
                           .Where(fq => fq.Condition == DateFilterQuery.ConditionalTypes.LessThenEqual)
                           .Select(qp => string.Format("&fq={0}:[* TO {1}]", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();
            
            var filterWildcardQueries = request.QueryOptions
                           .OfType<FilterQuery>()
                           .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.Contains)
                           .Select(qp => string.Format("&fq={0}:{1}", qp.ParameterName, FormatWildCard(qp.FieldValue))).ToList();
            
            var filterNotQueries = request.QueryOptions
                           .OfType<FilterQuery>()
                           .Where(fq => fq.Condition == FilterQuery.ConditionalTypes.NotEqual)
                           .Select(qp => string.Format("&fq=-{0}:{1}", qp.ParameterName, FormatToString(qp.FieldValue))).ToList();

            var disMaxQueryParms = request.QueryOptions
                           .OfType<DisMaxQueryParm>()
                           .Select(qp => ProcessQueryOption(qp.ParameterName, qp.ParameterValue)).ToList();

            var queryParms = request.QueryOptions
                           .OfType<QueryParm>()
                           .Select(qp => ProcessQueryOption(qp.ParameterName, qp.ParameterValue)).ToList();

            if (queryParms.Any())
            {
                queryOptions.Append(string.Join("", queryParms));
            }

            if (disMaxQueryParms.Any())
            {
                queryOptions.Append(string.Join("", disMaxQueryParms));
            }

            if (filterEqualQueries.Any())
                queryOptions.Append(string.Join("", filterEqualQueries));

            if (filterGreaterQueries.Any())
                queryOptions.Append(string.Join("", filterGreaterQueries));

            if (filterLessQueries.Any())
                queryOptions.Append(string.Join("", filterLessQueries));

            if (filterWildcardQueries.Any())
                queryOptions.Append(string.Join("", filterWildcardQueries));

            if (filterNotQueries.Any())
                queryOptions.Append(string.Join("", filterNotQueries));
                        
            var boostQueries = request.QueryOptions
                               .OfType<BoostQuery>()
                               .Select(qp => string.Format("&bq={0}:{1}^{2}", qp.ParameterName, qp.FieldValue, qp.Boost)).ToList();

            if (boostQueries.Any())
                queryOptions.Append(string.Join("", boostQueries));

            if(boostQueries.Any() || disMaxQueryParms.Any())
            {
                queryOptions.Append("&defType=dismax");
            }

            return queryOptions.ToString();
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

            var str = o.ToString(); //.Replace("/", " ").Replace("\\", " ").Trim();
            
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
        private static string AppendFacets(SearchRequest request)
        {
            var facets = new List<string>();

            var fieldFacets = request.Facets.OfType<FieldFacet>().ToList();
            
            foreach(var facet in fieldFacets)
            {
                facets.Add(string.Format("&facet.field={0}", facet.FieldName));
                //facets.Add(string.Format("&json.facet={{x:'hll({0})'}}", facet.FieldName)); 
                //facets.Add(string.Format("&json.facet={{categories: {{ type : terms, field : {0}, facet : {{ x : \"hll({0})\" }} }}}}", facet.FieldName)); 
            }

            var rangeFacets = request.Facets.OfType<NumRangeFacet>();

            foreach (var facet in rangeFacets)
            {
                foreach (var gap in facet.Range)
                {
                    if(gap.Lower == null)
                        continue;
                    
                    var lower = gap.Lower.Value;
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
                if (request.RefinementType == RefinementTypes.Refinement)
                {
                    facets.Add("&facet=on&facet.mincount=1");
                    
                }
                else
                {
                    facets.Add("&facet=on&facet.mincount=0");
                }
            }

            return string.Join("", facets);
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
