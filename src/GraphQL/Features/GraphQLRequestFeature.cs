namespace Tanka.GraphQL.Features;

internal class GraphQLRequestFeature : IGraphQLRequestFeature
{
    public required GraphQLRequest Request { get; set; }
}