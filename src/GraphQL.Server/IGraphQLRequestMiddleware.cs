namespace Tanka.GraphQL.Server;

public interface IGraphQLRequestMiddleware
{
    ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next);
}