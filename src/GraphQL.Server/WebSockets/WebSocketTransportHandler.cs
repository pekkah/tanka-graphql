using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets;

public partial class WebSocketTransportHandler(
    GraphQLRequestDelegate requestPipeline,
    HttpContext httpContext)
{
    private readonly ILoggerFactory _loggerFactory = httpContext
        .RequestServices
        .GetRequiredService<ILoggerFactory>();
    
    private readonly ILogger<WebSocketTransportHandler> _logger = httpContext
            .RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<WebSocketTransportHandler>();
    
    private WebSocketChannel _channel;
    private GraphQLTransportWSProtocol _protocol;

    [MemberNotNull(nameof(_channel))]
    [MemberNotNull(nameof(_protocol))]
    public async Task Handle(WebSocket webSocket)
    {
        _channel = new WebSocketChannel(webSocket, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        _protocol = new GraphQLTransportWSProtocol(
            new SubscriptionManager(
                httpContext,
                _channel.Writer,
                requestPipeline,
                _loggerFactory.CreateLogger<SubscriptionManager>()),
            _loggerFactory);
        
        Task readMessages = StartReading();
        await Task.WhenAll(_channel.Run(), readMessages);
    }

    private async Task StartReading()
    {
        try
        {
            while (await _channel.Reader.WaitToReadAsync(CancellationToken.None))
            {
                if (!_channel.Reader.TryRead(out MessageBase? message))
                    continue;

                var result = Accept(message);
                await result.Execute(new MessageContext(
                    _channel,
                    message,
                    requestPipeline)
                );
            }
        }
        catch(Exception x)
        {
            Log.ErrorWhileReadingMessages(_logger, x);
        }
        finally
        {
            _channel.Complete();
        }
    }

    private IMessageResult Accept(MessageBase message)
    {
        Log.ReceivedMessage(_logger, message);
        return _protocol.Accept(message);
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, "Received message '{Message}'")]
        public static partial void ReceivedMessage(
            ILogger logger, 
            [LogProperties]MessageBase message);

        [LoggerMessage(LogLevel.Error, "Error while reading messages from websocket")]
        public static partial void ErrorWhileReadingMessages(
            ILogger logger, 
            Exception exception);
    }
}