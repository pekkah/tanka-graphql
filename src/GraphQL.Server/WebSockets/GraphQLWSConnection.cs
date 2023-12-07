using System.Net.WebSockets;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tanka.GraphQL.Server.WebSockets.WebSocketPipe;

namespace Tanka.GraphQL.Server.WebSockets;

public partial class GraphQLWSConnection
{
    private readonly WebSocketMessageChannel<MessageBase> _channel;
    private readonly HttpContext _httpContext;
    private readonly ILogger<GraphQLWSConnection> _logger;
    private readonly WebSocket _webSocket;
    private bool _connectionInitReceived;

    public GraphQLWSConnection(
        WebSocket webSocket,
        GraphQLRequestDelegate requestDelegate,
        HttpContext httpContext)
    {
        _webSocket = webSocket;
        _httpContext = httpContext;
        _channel = new WebSocketMessageChannel<MessageBase>(webSocket,
            httpContext.RequestServices.GetRequiredService<ILogger<WebSocketMessageChannel<MessageBase>>>());
        Server = new ServerMethods(_channel, requestDelegate, httpContext);
        _logger = httpContext.RequestServices.GetRequiredService<ILogger<GraphQLWSConnection>>();
    }

    public ServerMethods Server { get; protected set; }

    public async Task Connect(CancellationToken cancellationToken)
    {
        Log.Connect(_logger, _httpContext.Connection.Id);
        using IDisposable? _ = _logger.BeginScope(_httpContext.Connection.Id);
        Task runTask = _channel.ProcessSocketAsync(cancellationToken);
        Task receiveTask = ReceiveMessages(cancellationToken);

        await Task.WhenAll(runTask, receiveTask);
    }

    private async Task HandleComplete(Complete complete, CancellationToken cancellationToken)
    {
        Log.MessageComplete(_logger, complete);
        await Server.Complete(complete, cancellationToken);
    }

    private async Task HandleMessage(MessageBase message, CancellationToken cancellationToken)
    {
        Task task = message switch
        {
            ConnectionInit => TooManyInitializationRequests(),
            Subscribe subscribe => HandleSubscribe(subscribe, cancellationToken),
            Ping ping => HandlePing(ping, cancellationToken),
            Complete complete => HandleComplete(complete, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(message), message, null)
        };

        await task;
    }

    private async Task HandlePing(Ping ping, CancellationToken cancellationToken)
    {
        Log.MessagePing(_logger, ping);
        await WriteMessage(new Pong(), cancellationToken);
    }

    private async Task HandleSubscribe(Subscribe subscribe, CancellationToken cancellationToken)
    {
        Log.MessageSubscribe(_logger, subscribe);
        await Server.Subscribe(subscribe, cancellationToken);
    }

    private async Task ReceiveMessages(CancellationToken cancellationToken)
    {
        try
        {
            if (!_connectionInitReceived)
            {
                MessageBase message = await _channel.Reader.ReadAsync(cancellationToken);

                if (message is not ConnectionInit initMessage)
                {
                    await _webSocket.CloseOutputAsync(CloseCode.Unauthorized, "Expected connection_init messsage",
                        CancellationToken.None);
                    Log.ExpectedInitMessageGot(_logger, message.Type);
                    return;
                }

                _connectionInitReceived = true;
                await Server.ConnectionInit(initMessage, cancellationToken);
            }

            while (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                MessageBase message = await _channel.Reader.ReadAsync(cancellationToken);

                await HandleMessage(message, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // noop
            Log.OperationCancelled(_logger);
        }
    }

    private async Task TooManyInitializationRequests()
    {
        Log.TooManyInitializationRequests(_logger);
        await _channel.Complete(CloseCode.TooManyInitialisationRequests);
    }

    private async Task WriteMessage(MessageBase message, CancellationToken cancellationToken)
    {
        Log.MessageWrite(_logger, message);
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Connected: {connectionId}")]
        public static partial void Connect(ILogger logger, string connectionId);

        [LoggerMessage(8, LogLevel.Error, "Expected 'connection_init' got '{actualMessageType}'")]
        public static partial void ExpectedInitMessageGot(ILogger logger, string actualMessageType);

        [LoggerMessage(2, LogLevel.Information, "Complete: {complete}")]
        public static partial void MessageComplete(ILogger logger, Complete complete);

        [LoggerMessage(3, LogLevel.Information, "Ping: {ping}")]
        public static partial void MessagePing(ILogger logger, Ping ping);

        [LoggerMessage(4, LogLevel.Information, "Subscribe: {subscribe}")]
        public static partial void MessageSubscribe(ILogger logger, Subscribe subscribe);

        [LoggerMessage(7, LogLevel.Information, "Writing: {message}")]
        public static partial void MessageWrite(ILogger logger, MessageBase message);

        [LoggerMessage(5, LogLevel.Warning, "Operation cancelled")]
        public static partial void OperationCancelled(ILogger logger);

        [LoggerMessage(6, LogLevel.Error, "Too many initialization requests")]
        public static partial void TooManyInitializationRequests(ILogger logger);
    }
}