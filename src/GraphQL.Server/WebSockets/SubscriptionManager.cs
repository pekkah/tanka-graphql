using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tanka.GraphQL.Request;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Server.WebSockets;

public partial class SubscriptionManager(
    HttpContext httpContext,
    ChannelWriter<MessageBase> writer,
    GraphQLRequestDelegate requestDelegate,
    ILogger<SubscriptionManager> logger)
{
    private readonly ConcurrentDictionary<string, (CancellationTokenSource Unsubscribe, Task Execute)>
        _subscriptions = new ();
    
    public bool Enqueue(string id, GraphQLHttpRequest request)
    {
        if (_subscriptions.ContainsKey(id))
        {
            Log.SubscriberAlreadyExists(logger, id);
            return false;
        }

        //todo: do we need locking here as the TryAdd can fail, but we already started...
        CancellationTokenSource unsubscribe = new();
        _subscriptions.TryAdd(id, (unsubscribe, Query(
            id,
            request,
            writer,
            httpContext,
            requestDelegate,
            unsubscribe.Token)));

        return true;
    }

    public async Task Dequeue(string id)
    {
        if (_subscriptions.TryRemove(id, out (CancellationTokenSource Unsubscribe, Task Execute) subscription))
        {
            try
            {
                await subscription.Unsubscribe.CancelAsync();
                await subscription.Execute;
            }
            finally
            {
                subscription.Unsubscribe.Dispose();
            }
        }
    }

    private static async Task Query(
        string subscriptionId,
        GraphQLHttpRequest request, 
        ChannelWriter<MessageBase> writer,
        HttpContext httpContext,
        GraphQLRequestDelegate requestDelegate,
        CancellationToken cancellationToken)
    {
        var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger<SubscriptionManager>();
        
        using var _ = logger.BeginScope("Subscription({SubscriptionId})", subscriptionId);
        
        var context = new GraphQLRequestContext
        {
            HttpContext = httpContext,
            RequestCancelled = cancellationToken,
            RequestServices = httpContext.RequestServices,
            Request = new GraphQLRequest
            {
                InitialValue = null,
                Query = request.Query,
                OperationName = request.OperationName,
                Variables = request.Variables
            }
        };

        try
        {
            ulong count = 0;
            Log.Request(logger, subscriptionId, context.Request);

            // execute request context
            await requestDelegate(context);

            // get result stream
            await using var enumerator =
                context.Response.WithCancellation(cancellationToken)
                    .GetAsyncEnumerator();

            long started = Stopwatch.GetTimestamp();
            while (await enumerator.MoveNextAsync())
            {
                count++;
                string elapsed = $"{Stopwatch.GetElapsedTime(started).TotalMilliseconds}ms";
                Log.ExecutionResult(logger, subscriptionId, enumerator.Current, elapsed);
                await writer.WriteAsync(new Next
                {
                    Id = subscriptionId, 
                    Payload = enumerator.Current
                });
                started = Stopwatch.GetTimestamp();
            }

            

            Log.Completed(logger, subscriptionId, count);
        }
        catch (OperationCanceledException)
        {
            // noop
        }
        catch (ValidationException x)
        {
            ValidationResult validationResult = x.Result;
            await writer.WriteAsync(
                new Error
                {
                    Id = subscriptionId,
                    Payload = validationResult.Errors
                        .Select(ve => ve.ToError())
                        .ToArray()
                });
        }
        catch (QueryException x)
        {
            await writer.WriteAsync(
                new Error
                {
                    Id = subscriptionId,
                    Payload =
                    [
                        context.Errors?.FormatError(x)!
                    ]
                });
        }
        catch (Exception x)
        {
            await writer.WriteAsync(
                new Error
                {
                    Id = subscriptionId,
                    Payload =
                    [
                        context.Errors?.FormatError(x)!
                    ]
                });
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                await writer.WriteAsync(new Complete
                {
                    Id = subscriptionId
                });
        }
    }

    public static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Expected 'subscribe' got '{type}'")]
        public static partial void ExpectedSubscribeMessageGot(ILogger logger, string type);

        [LoggerMessage(5, LogLevel.Debug, "Subscription({Id}) - Result({elapsed}): {result}")]
        public static partial void ExecutionResult(ILogger logger, string id, ExecutionResult? result, string elapsed);

        [LoggerMessage(3, LogLevel.Debug, "Subscription({Id}) - Request: {request}")]
        public static partial void Request(ILogger logger, string id, GraphQLRequest request);

        [LoggerMessage(10, LogLevel.Information,
            "Subscription({Id}) - Server stream completed. {count} messages sent.")]
        public static partial void Completed(ILogger logger, string id, ulong count);

        [LoggerMessage(0, LogLevel.Error,
            "Subscription({Id}) - Subscription id  is not unique")]
        public static partial void SubscriberAlreadyExists(ILogger logger, string id);

        [LoggerMessage(LogLevel.Information,
            "Subscription({Id}) - Complete client subscription.")]
        public static partial void Complete(ILogger logger, string id);
    }
}