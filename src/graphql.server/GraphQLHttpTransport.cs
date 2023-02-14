using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Tanka.GraphQL.Server;

public class GraphQLHttpTransport : IGraphQLTransport
{
    public IEndpointConventionBuilder Map(
        string pattern,
        IEndpointRouteBuilder routes,
        GraphQLRequestDelegate requestDelegate)
    {
        return new RouteHandlerBuilder(new[]
        {
            routes.MapPost(pattern, ProcessRequest(requestDelegate)),
            routes.MapGet(pattern, ProcessRequest(requestDelegate))
        });
    }

    public void Build(GraphQLRequestPipelineBuilder builder)
    {
        builder.UseHttpTransport();
    }

    private RequestDelegate ProcessRequest(GraphQLRequestDelegate pipeline)
    {
        return async httpContext =>
        {
            if (!httpContext.WebSockets.IsWebSocketRequest
                && httpContext.Request.HasJsonContentType())
            {
                var context = new GraphQLRequestContext
                {
                    RequestServices = httpContext.RequestServices,
                    RequestCancelled = httpContext.RequestAborted
                };

                context.Features.Set<IHttpContextFeature>(new HttpContextFeature
                {
                    HttpContext = httpContext
                });

                await pipeline(context);
            }
        };
    }
}