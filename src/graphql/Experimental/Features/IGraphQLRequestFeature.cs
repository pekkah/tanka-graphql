namespace Tanka.GraphQL.Experimental.Features;

public interface IGraphQLRequestFeature
{
    public GraphQLRequest Request { get; set; }
}