using Tanka.GraphQL.Features;

namespace Tanka.GraphQL.Request;

internal class GraphQLRequestFeature : IGraphQLRequestFeature
{
    public GraphQLRequest? Request { get; set; }
}