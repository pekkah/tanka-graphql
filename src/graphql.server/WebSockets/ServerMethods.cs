using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tanka.GraphQL.Server.WebSockets.WebSocketPipe;

namespace Tanka.GraphQL.Server.WebSockets;

public class ServerMethods
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
    }

    public ClientMethods Client { get; set; }

    public ConcurrentDictionary<string, (CancellationTokenSource Unsubscribe, Task Worker)> Subscriptions = new();

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
        var cancellationToken = unsubscribeOrAborted.Token;
        var context = new GraphQLRequestContext();

        //todo: replace this with context.RequestService = _httpContext.RequestServices
        context.RequestServices = _httpContext.RequestServices;
        context.Request = new()
        {
            InitialValue = null,
            Document = subscribe.Payload.Query,
            OperationName = subscribe.Payload.OperationName,
            Variables = subscribe.Payload.Variables
        };

        await using var enumerator = _requestDelegate(context).WithCancellation(cancellationToken)
            .GetAsyncEnumerator();

        try
        {
            while (await enumerator.MoveNextAsync())
            {
                await Client.Next(new Next()
                {
                    Id = subscribe.Id,
                    Payload = enumerator.Current
                }, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await Client.Complete(new Complete()
                {
                    Id = subscribe.Id
                }, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // noop
        }
        catch (QueryException x)
        {
            await Client.Error(new Error()
            {
                Id = subscribe.Id,
                Payload = new[]
                {
                    new ExecutionError()
                    {
                        Path = x.Path.Segments.ToArray(),
                        Message = x.Message
                    }
                }
            }, cancellationToken);
        }
        finally
        {
            unsubscribeOrAborted.Cancel();
            Subscriptions.TryRemove(subscribe.Id, out _);
        }
    }

    public async Task Complete(Complete complete, CancellationToken cancellationToken)
    {
        if (Subscriptions.TryRemove(complete.Id, out var pair))
        {
            var (unsubscribe, worker) = pair;

            unsubscribe.Cancel();
            await worker;
        }
    }
}