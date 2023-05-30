using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server;

public partial class GraphQLHttpTransport : IGraphQLTransport
{
    private readonly ILogger<GraphQLHttpTransport> _logger;

    public GraphQLHttpTransport(ILogger<GraphQLHttpTransport> logger)
    {
        _logger = logger;
    }

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
                Log.BeginRequest(_logger);
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
                Log.EndRequest(_logger);
            }
        };
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information,  "Processing GraphQL Http request")]
        public static partial void BeginRequest(ILogger logger);

        [LoggerMessage(int.MaxValue, LogLevel.Information, "Processing GraphQL Http request completed")]
        public static partial void EndRequest(ILogger logger);
    }
}