using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Tanka.GraphQL.Server;

public class GraphQLHttpTransport : IGraphQLTransport
{
    public IEndpointConventionBuilder Map(
        string pattern,
        IEndpointRouteBuilder routes,
        GraphQLRequestPipelineBuilder pipelineBuilder)
    {
        return new RouteHandlerBuilder(new[]
        {
            routes.MapPost(pattern, CreateRequestDelegate(pipelineBuilder)),
            routes.MapGet(pattern, CreateRequestDelegate(pipelineBuilder))
        });
    }

    private RequestDelegate CreateRequestDelegate(
        GraphQLRequestPipelineBuilder pipelineBuilder)
    {
        var pipeline = pipelineBuilder.Build();
        return ProcessRequest(pipeline);
    }

    private RequestDelegate ProcessRequest(GraphQLRequestDelegate pipeline)
    {
        return async httpContext =>
        {
            if (!httpContext.WebSockets.IsWebSocketRequest
                && httpContext.Request.HasJsonContentType())
            {
                var context = new GraphQLRequestContext();
                context.RequestServices = httpContext.RequestServices;

                var stopwatch = Stopwatch.StartNew();

                // Parse request
                var request = await httpContext.Request.ReadFromJsonAsync<GraphQLHttpRequest>();

                if (request is null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Detail = "Could not parse GraphQL request from body of the request"
                    });

                    return;
                }

                context.Request = new()
                {
                    InitialValue = null,
                    Document = request.Query,
                    OperationName = request.OperationName,
                    Variables = request.Variables
                };

                var enumerator = pipeline(context).GetAsyncEnumerator();

                if (await enumerator.MoveNextAsync())
                {
                    var initialResult = enumerator.Current;

                    if (await enumerator.MoveNextAsync())
                        throw new InvalidOperationException("HttpTransport does not support multiple responses.");

                    stopwatch.Stop();
                    httpContext.Response.Headers["Elapsed"] = new($"{stopwatch.Elapsed.TotalSeconds}s");
                    await httpContext.Response.WriteAsJsonAsync(initialResult);
                }
            }
        };
    }
}