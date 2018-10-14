namespace fugu.graphql.server
{
    public class Request
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public QueryOperation Operation { get; set; }
    }
}