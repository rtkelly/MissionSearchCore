using MissionSearch.Search.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionSearch
{
    public static class SearchRequestExtension
    {
        public static SearchResponse<T> Search<T>(this SearchRequest request) where T : ISearchDocument
        {
            return SearchFactory<T>.SearchClient.Search(request);
        }

        public static SearchRequest FilterByDateRange(this SearchRequest request, string fieldName, DateTime gtd, DateTime ltd)
        {
            request.QueryOptions.Add(new RangeQuery<DateTime>(fieldName, gtd, ltd));

            return request;
        }

        public static SearchRequest FilterByTerm(this SearchRequest request, string fieldName, string fieldValue)
        {
            request.QueryOptions.Add(new FilterQuery(fieldName, fieldValue));

            return request;
        }

        public static SearchRequest FilterByTerm(this SearchRequest request, string fieldName, string fieldValue, FilterQuery.ConditionalTypes condition)
        {
            request.QueryOptions.Add(new FilterQuery(fieldName, condition, fieldValue));

            return request;
        }

        public static SearchRequest AddCategoryFacet(this SearchRequest request, string fieldName, string categoryName, string fieldLabel, RefinementType refinementOption)
        {
            request.Facets.Add(new CategoryFacet(fieldName, categoryName, fieldLabel, refinementOption));

            return request;
        }

        public static SearchRequest AddCategoryFacet(this SearchRequest request, string fieldName, string categoryName, RefinementType refinementOption)
        {
            return request.AddCategoryFacet(fieldName, categoryName, categoryName, refinementOption);
        }

        public static SearchRequest AddCategoryFacet(this SearchRequest request, string fieldName, string categoryName)
        {
            return request.AddCategoryFacet(fieldName, categoryName, categoryName, RefinementType.Refinement);
        }

        public static SearchRequest AddTermFacet(this SearchRequest request, string fieldName, string fieldLabel, RefinementType refinementOption)
        {
            request.Facets.Add(new FieldFacet(fieldName, fieldLabel, refinementOption));

            return request;
        }

        public static SearchRequest AddTermFacet(this SearchRequest request, string fieldName, string fieldLabel)
        {
            request.Facets.Add(new FieldFacet(fieldName, fieldLabel, RefinementType.Refinement));

            return request;
        }
        public static SearchRequest AddSort(this SearchRequest request, string fieldName, SortOrder.SortOption sortOption)
        {
            if (request.Sort == null)
            {
                request.Sort = new System.Collections.Generic.List<SortOrder>();
            }

            request.Sort.Add(new SortOrder(fieldName, sortOption));

            return request;
        }

        public static SearchRequest AddDateRangeFacet(this SearchRequest request, string fieldName, string fieldLabel)
        {
            return request.AddDateRangeFacet(fieldName, fieldLabel, RefinementType.Refinement);
        }

        public static SearchRequest AddDateRangeFacet(this SearchRequest request, string fieldName, string fieldLabel, RefinementType refinementOption)
        {
            var dateFacet = new DateRangeFacet(fieldName, fieldLabel, refinementOption);

            request.Facets.Add(dateFacet);

            var seedDate = new DateTime(DateTime.Today.Year, 1, 1);

            dateFacet.Ranges.Add(new DateRange(seedDate, seedDate.AddYears(1), seedDate.Year));
            dateFacet.Ranges.Add(new DateRange(seedDate.AddYears(-1), seedDate, seedDate.AddYears(-1).Year));
            dateFacet.Ranges.Add(new DateRange(seedDate.AddYears(-2), seedDate.AddYears(-1), seedDate.AddYears(-2).Year));
            dateFacet.Ranges.Add(new DateRange(null, seedDate.AddYears(-2), seedDate.AddYears(-3).Year));


            return request;
        }

        public static SearchRequest AddBoostQuery(this SearchRequest request, string fieldName, string fieldValue, int boost)
        {
            request.QueryOptions.Add(new BoostQuery(fieldName, fieldValue, boost));

            return request;
        }

        public static SearchRequest AddPivotFacet(this SearchRequest request, string fieldName, string fieldLabel)
        {
            request.Facets.Add(new PivotFacet(fieldName, fieldLabel));

            return request;
        }
        

    }
}
