using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.subscriptions;
using fugu.graphql.type;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.server
{
    public class ServerHub : Hub<IServerClient>
    {
        private readonly SubscriptionServerManager _servers;

        public ServerHub(SubscriptionServerManager servers)
        {
            _servers = servers;
        }

        public override Task OnConnectedAsync()
        {
            _servers.OnConnected(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _servers.OnDisconnected(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public ChannelReader<OperationMessage> Connect()
        {
            return _servers.GetReader(Context.ConnectionId);
        }

        public Task Execute(Request request)
        {
            return _servers.Execute(Context.ConnectionId, request);
        }

    }

    public class SubscriptionServerManager
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISchema _schema;

        public SubscriptionServerManager(ISchema schema, ILoggerFactory loggerFactory)
        {
            _schema = schema;
            _loggerFactory = loggerFactory;
            Servers = new ConcurrentDictionary<string, SubscriptionServer>();
        }

        public ConcurrentDictionary<string, SubscriptionServer> Servers { get; set; }

        public void OnConnected(string connectionId)
        {
            var transport = new HubClientTransport();
            var server = new SubscriptionServer(
                transport,
                new SubscriptionManager(
                    new SchemaExecutor(_schema),
                    _loggerFactory),
                new IOperationMessageListener[]
                {
                    new LogMessagesListener(_loggerFactory.CreateLogger<LogMessagesListener>()),
                    new ApolloProtocol(_loggerFactory.CreateLogger<ApolloProtocol>())
                },
                _loggerFactory.CreateLogger<SubscriptionServer>()
            );

            Task.Factory.StartNew(async () => await server.OnConnect());
            Servers[connectionId] = server;
        }

        public async Task OnDisconnected(string connectionId)
        {
            Servers.Remove(connectionId, out var server);

            server.Transport.Complete();
            await server.Transport.Completion;
        }

        public ChannelReader<OperationMessage> GetReader(string connectionId)
        {
            var server = Servers[connectionId];
            var hubClientTransport = (HubClientTransport) server.Transport;
            return hubClientTransport.MessageChannel.Reader;
        }

        public Task Execute(string connectionId, Request request)
        {
            var server = Servers[connectionId];
            var hubClientTransport = (HubClientTransport) server.Transport;

            var payload = request.Operation != null ? JObject.FromObject(new OperationMessagePayload
            {
                OperationName = request.Operation.OperationName,
                Query = request.Operation.Query,
                Variables = request.Operation.Variables,
                Extensions = request.Operation.Extensions
            }): null;

            return hubClientTransport.ConsumeMessage(new OperationMessage
            {
                Id = request.Id,
                Type = request.Type,
                Payload = payload
            });
        }
    }

    public class Request
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public QueryOperation Operation { get; set; }
    }

    public class HubClientTransport : IMessageTransport
    {
        private readonly BufferBlock<OperationMessage> _receivedMessages;

        public HubClientTransport()
        {
            MessageChannel = Channel.CreateUnbounded<OperationMessage>();
            Writer = new ActionBlock<OperationMessage>(async message =>
            {
                await MessageChannel.Writer.WriteAsync(message);
            });
            _receivedMessages = new BufferBlock<OperationMessage>();
        }

        public Channel<OperationMessage> MessageChannel { get; set; }

        public ISourceBlock<OperationMessage> Reader => _receivedMessages;

        public ITargetBlock<OperationMessage> Writer { get; }

        public void Complete()
        {
            MessageChannel.Writer.Complete();
        }

        public Task Completion => MessageChannel.Reader.Completion;

        public Task ConsumeMessage(OperationMessage operation)
        {
            return _receivedMessages.SendAsync(operation);
        }
    }

    public interface IServerClient
    {
    }

    public class QueryOperation
    {
        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }
}