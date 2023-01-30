using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Tanka.GraphQL.Server.WebSockets;

/// <summary>
///     WebSocket transport compliant with graphql-ws protocol
///     described here https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md
/// </summary>
public class GraphQLWSTransport : IGraphQLTransport
{
    /// <summary>
    ///     Due to historical reasons this actually is the protocol name used
    ///     by the newer protocol.
    /// </summary>
    public static string SubProcol = "graphql-transport-ws";

    public IEndpointConventionBuilder Map(
        string pattern,
        IEndpointRouteBuilder routes,
        GraphQLRequestPipelineBuilder pipelineBuilder)
    {
        return routes.Map(pattern + "/ws", CreateRequestDelegate(pipelineBuilder));
    }

    private RequestDelegate CreateRequestDelegate(GraphQLRequestPipelineBuilder pipelineBuilder)
    {
        var pipeline = pipelineBuilder.Build();
        return ProcessRequest(pipeline);
    }

    private RequestDelegate ProcessRequest(GraphQLRequestDelegate pipeline)
    {
        return async httpContext =>
        {
            if (!httpContext.WebSockets.IsWebSocketRequest)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Detail = "Could handle connection. WebSockets request expected."
                });

                return;
            }

            if (httpContext.WebSockets.WebSocketRequestedProtocols?.Contains(SubProcol) == false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Detail = $"Request does not contain sub-protocol '{SubProcol}'."
                });

                return;
            }

            var webSocket = await httpContext.WebSockets
                .AcceptWebSocketAsync(SubProcol);

            await HandleProtocol(httpContext, webSocket, pipeline);
        };
    }

    private async Task HandleProtocol(
        HttpContext httpContext,
        WebSocket webSocket,
        GraphQLRequestDelegate requestPipeline)
    {
        var connection = new GraphQLWSConnection(webSocket, requestPipeline, httpContext);
        await connection.Connect(httpContext.RequestAborted);
    }
}