namespace Tanka.GraphQL.Server;

public static class HttpTransportGraphQLRequestBuilderExtensions
{
    public static GraphQLRequestPipelineBuilder UseHttpTransport(this GraphQLRequestPipelineBuilder builder)
    {
        return builder.Use<GraphQLHttpTransportMiddleware>();
    }
}