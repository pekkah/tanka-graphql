using Tanka.GraphQL.Request;

namespace Tanka.GraphQL.Features;

public interface IGraphQLRequestFeature
{
    public GraphQLRequest Request { get; set; }
}