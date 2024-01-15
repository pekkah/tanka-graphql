using System.Collections.Concurrent;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tanka.GraphQL.Request;
using Tanka.GraphQL.Server.WebSockets.WebSocketPipe;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Server.WebSockets;

public partial class ServerMethods
{
    private readonly GraphQLRequestDelegate _requestDelegate;
    private readonly HttpContext _httpContext;
    protected WebSocketMessageChannel<MessageBase> Channel { get; }

    public ServerMethods(WebSocketMessageChannel<MessageBase> channel, GraphQLRequestDelegate requestDelegate, HttpContext httpContext)
    {
        _requestDelegate = requestDelegate;
        _httpContext = httpContext;
        Channel = channel;
        Client = new ClientMethods(Channel.Writer);
        _logger = httpContext.RequestServices.GetRequiredService<ILogger<ServerMethods>>();
    }

    public ClientMethods Client { get; set; }

    public ConcurrentDictionary<string, (CancellationTokenSource Unsubscribe, Task Worker)> Subscriptions = new();
    
    private readonly ILogger<ServerMethods> _logger;

    public async Task ConnectionInit(ConnectionInit connectionInit, CancellationToken cancellationToken)
    {
        await Client.ConnectionAck(new ConnectionAck(), cancellationToken);
    }

    public async Task Subscribe(Subscribe subscribe, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscribe.Id);

        if (Subscriptions.ContainsKey(subscribe.Id))
        {
            await Channel.Complete(
                CloseCode.SubscriberAlreadyExists,
                $"Subscriber for {subscribe.Id} already exists");

            return;
        }

        var unsubscribe = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (!Subscriptions.TryAdd(subscribe.Id, (unsubscribe, Execute(subscribe, unsubscribe))))
        {
            unsubscribe.Cancel(false);
            await Channel.Complete(
                CloseCode.SubscriberAlreadyExists,
                $"Subscriber for {subscribe.Id} already exists");
        }
    }

    private async Task Execute(Subscribe subscribe, CancellationTokenSource unsubscribeOrAborted)
    {
        _ = _logger.BeginScope(subscribe.Id);
        var cancellationToken = unsubscribeOrAborted.Token;
        var context = new GraphQLRequestContext
        {
            HttpContext = _httpContext,
            RequestServices = _httpContext.RequestServices,
            Request = new()
            {
                InitialValue = null,
                Query = subscribe.Payload.Query,
                OperationName = subscribe.Payload.OperationName,
                Variables = subscribe.Payload.Variables
            }
        };
        
        try
        {
            ulong count = 0;
            Log.Request(_logger, subscribe.Id, context.Request);
            await _requestDelegate(context);
            await using var enumerator = context.Response.GetAsyncEnumerator(cancellationToken);

            long started = Stopwatch.GetTimestamp();
            while (await enumerator.MoveNextAsync())
            {
                count++;
                string elapsed = $"{Stopwatch.GetElapsedTime(started).TotalMilliseconds}ms";
                Log.ExecutionResult(_logger, subscribe.Id, enumerator.Current, elapsed);
                await Client.Next(new Next() { Id = subscribe.Id, Payload = enumerator.Current }, cancellationToken);
                started = Stopwatch.GetTimestamp();
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await Client.Complete(new Complete() { Id = subscribe.Id }, cancellationToken);
            }
            
            Log.Completed(_logger, subscribe.Id, count);
        }
        catch (OperationCanceledException)
        {
            // noop
        }
        catch (ValidationException x)
        {
            var validationResult = x.Result;
            await Client.Error(
                new Error()
                {
                    Id = subscribe.Id, 
                    Payload = validationResult.Errors.Select(ve => ve.ToError()).ToArray()
                }, cancellationToken);
        }
        catch (QueryException x)
        {
            await Client.Error(
                new Error()
                {
                    Id = subscribe.Id,
                    Payload = new[]
                    {
                        context.Errors?.FormatError(x)!
                    }
                }, cancellationToken);
        }
        catch (Exception x)
        {
            await Client.Error(
                new Error()
                {
                    Id = subscribe.Id,
                    Payload = new[]
                    {
                        context.Errors?.FormatError(x)!
                    }
                }, cancellationToken);
        }
        finally
        {
            await unsubscribeOrAborted.CancelAsync();
            Subscriptions.TryRemove(subscribe.Id, out _);
        }
    }

    public async Task Complete(Complete complete, CancellationToken cancellationToken)
    {
        if (Subscriptions.TryRemove(complete.Id, out var pair))
        {
            var (unsubscribe, worker) = pair;

            await unsubscribe.CancelAsync();
            await worker;
        }
    }

    private static partial class Log
    {
        [LoggerMessage(5, LogLevel.Debug, "Subscription({Id}) - Result({elapsed}): {result}")]
        public static partial void ExecutionResult(ILogger logger, string id, ExecutionResult? result, string elapsed);

        [LoggerMessage(3, LogLevel.Debug, "Subscription({Id}) - Request: {request}")]
        public static partial void Request(ILogger logger, string id, GraphQLRequest request);

        [LoggerMessage(10, LogLevel.Information, "Subscription({Id}) - Server stream completed. {count} messages sent.")]
        public static partial void Completed(ILogger logger, string id, ulong count);
    }
}