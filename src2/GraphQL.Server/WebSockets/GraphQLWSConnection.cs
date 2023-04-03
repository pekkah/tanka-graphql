using System;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Tanka.GraphQL.Server.WebSockets.WebSocketPipe;
using static System.Net.Mime.MediaTypeNames;

namespace Tanka.GraphQL.Server.WebSockets;

public class GraphQLWSConnection
{
    private readonly WebSocket _webSocket;
    private readonly HttpContext _httpContext;
    private bool _connectionInitReceived;
    private readonly WebSocketMessageChannel<MessageBase> _channel;

    public GraphQLWSConnection(
        WebSocket webSocket,
        GraphQLRequestDelegate requestDelegate,
        HttpContext httpContext)
    {
        _webSocket = webSocket;
        _httpContext = httpContext;
        _channel = new WebSocketMessageChannel<MessageBase>(webSocket, new NullLogger<WebSocketMessageChannel<MessageBase>>());
        Server = new ServerMethods(_channel, requestDelegate, httpContext);
    }

    public async Task Connect(CancellationToken cancellationToken)
    {
        var runTask = _channel.ProcessSocketAsync(cancellationToken);
        var receiveTask = ReceiveMessages(cancellationToken);

        await Task.WhenAll(runTask, receiveTask);
    }

    public ServerMethods Server { get; protected set; }

    private async Task ReceiveMessages(CancellationToken cancellationToken)
    {
        try
        {
            if (!_connectionInitReceived)
            {
                var message = await _channel.Reader.ReadAsync(cancellationToken);

                if (message is not ConnectionInit initMessage)
                {
                    await _webSocket.CloseOutputAsync(CloseCode.Unauthorized, "Expected connection_init messsage",
                        CancellationToken.None);
                    return;
                }

                _connectionInitReceived = true;
                await Server.ConnectionInit(initMessage, cancellationToken);
            }

            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                var message = await _channel.Reader.ReadAsync(cancellationToken);

                await HandleMessage(message, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // noop
        }
    }

    private async Task WriteMessage(MessageBase message, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    private async Task HandleMessage(MessageBase message, CancellationToken cancellationToken)
    {
        var task = message switch
        {
            ConnectionInit => TooManyInitializationRequests(),
            Subscribe subscribe => HandleSubscribe(subscribe, cancellationToken),
            Ping ping => HandlePing(ping, cancellationToken),
            Complete complete => HandleComplete(complete, cancellationToken)
        };

        await task;
    }

    private async Task HandleComplete(Complete complete, CancellationToken cancellationToken)
    {
        await Server.Complete(complete, cancellationToken);
    }

    private async Task HandleSubscribe(Subscribe subscribe, CancellationToken cancellationToken)
    {
        await Server.Subscribe(subscribe, cancellationToken);
    }

    private async Task TooManyInitializationRequests()
    {
        await _channel.Complete(CloseCode.TooManyInitialisationRequests);
    }

    private async Task HandlePing(Ping ping, CancellationToken cancellationToken)
    {
        await WriteMessage(new Pong(), cancellationToken);
    }
}