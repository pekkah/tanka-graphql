namespace Tanka.GraphQL.Experimental.Features;

internal class GraphQLRequestFeature : IGraphQLRequestFeature
{
    public required GraphQLRequest Request { get; set; }
}