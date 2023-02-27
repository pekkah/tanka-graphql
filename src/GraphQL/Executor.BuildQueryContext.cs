namespace Tanka.GraphQL;

public partial class Executor
{
    public QueryContext BuildQueryContextAsync(GraphQLRequest request)
    {
        var queryContext = new QueryContext
        {
            Request = request
        };

        return queryContext;
    }
}