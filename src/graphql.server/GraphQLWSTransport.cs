using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Devlooped.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server;

/// <summary>
///     WebSocket transport compliant with graphql-ws protocol
///     described here https://github.com/enisdenjo/graphql-ws/blob/master/PROTOCOL.md
/// </summary>
public class GraphQLWSTransport : IGraphQLTransport
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

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
        return routes.Map(pattern+"/ws", CreateRequestDelegate(pipelineBuilder));
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
        var webSocketPipe = WebSocketPipe.Create(webSocket, true);

        var runTask = webSocketPipe.RunAsync(httpContext.RequestAborted);

        /* Handle connection init message */
        var connectionInitMessage = await ConnectionInit(webSocketPipe, httpContext.RequestAborted);

        if (connectionInitMessage is null)
        {
            await webSocketPipe.CompleteAsync(WebSocketCloseStatus.InvalidMessageType,
                "connection_init message not received.");

        }


        await runTask;
    }

    private async Task<ConnectionInit?> ConnectionInit(
        IWebSocketPipe webSocketPipe,
        CancellationToken cancellationToken)
    {
        var message = await ReadMessage(webSocketPipe, cancellationToken);
        return message as ConnectionInit;
    }

    private async Task<MessageBase?> ReadMessage(IWebSocketPipe webSocketPipe, CancellationToken cancellationToken)
    {

        var input = webSocketPipe.Input;

        while (!cancellationToken.IsCancellationRequested && await input.ReadAsync(cancellationToken) is var result)
        {
            if (result.IsCompleted)
                break;

            var message = ReadMessageCore(result.Buffer, cancellationToken);

            input.AdvanceTo(result.Buffer.End);
            return message;
        }

        return null;
    }

    private MessageBase? ReadMessageCore(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();


        var reader = new Utf8JsonReader(buffer);
        var message = JsonSerializer.Deserialize<MessageBase>(ref reader, JsonOptions);

        return message;
    }
}