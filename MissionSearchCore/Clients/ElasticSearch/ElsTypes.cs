namespace MissionSearch.Clients.ElasticSearch
{
    public static partial class Els
    {
        public enum Clause { term, terms, range };

        public enum BoolQuery { must, mustnot, should, filter };
    }
}
