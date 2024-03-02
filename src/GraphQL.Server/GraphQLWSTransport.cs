using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server;

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
    public static string SubProtocol = "graphql-transport-ws";

    public IEndpointConventionBuilder Map(string pattern, IEndpointRouteBuilder routes,
        GraphQLRequestDelegate requestDelegate)
    {
        return routes.Map(pattern + "/ws", ProcessRequest(requestDelegate));
    }

    public void Build(GraphQLRequestPipelineBuilder builder)
    {
    }

    private async Task HandleProtocol(
        HttpContext httpContext,
        WebSocket webSocket,
        GraphQLRequestDelegate requestPipeline)
    {
        var handler = new WebSocketTransportHandler(
            requestPipeline,
            httpContext);

        await handler.Handle(webSocket);
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

            if (httpContext.WebSockets.WebSocketRequestedProtocols?.Contains(EchoProtocol.Protocol) == true)
            {
                using WebSocket echoWebSocket = await httpContext.WebSockets
                    .AcceptWebSocketAsync(EchoProtocol.Protocol);

                await EchoProtocol.Run(echoWebSocket);
                return;
            }

            if (httpContext.WebSockets.WebSocketRequestedProtocols?.Contains(SubProtocol) == false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Detail = $"Request does not contain sub-protocol '{SubProtocol}'."
                });

                return;
            }

            using WebSocket webSocket = await httpContext.WebSockets
                .AcceptWebSocketAsync(SubProtocol);

            await HandleProtocol(httpContext, webSocket, pipeline);
        };
    }
}