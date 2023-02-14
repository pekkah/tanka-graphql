using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Tanka.GraphQL.Server;

public interface IGraphQLTransport
{
    IEndpointConventionBuilder Map(
        string pattern, 
        IEndpointRouteBuilder routes, 
        GraphQLRequestDelegate requestDelegate);

    void Build(GraphQLRequestPipelineBuilder builder);
}