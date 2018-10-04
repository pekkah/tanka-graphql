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

namespace fugu.graphql.server
{
    public class ServerHub : Hub<IServerClient>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISchema _schema;

        public ServerHub(ISchema schema, ILoggerFactory loggerFactory)
        {
            _schema = schema;
            _loggerFactory = loggerFactory;
            Servers = new ConcurrentDictionary<string, SubscriptionServer>();
        }

        public ConcurrentDictionary<string, SubscriptionServer> Servers { get; }

        public override Task OnConnectedAsync()
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

            Servers[Context.ConnectionId] = server;
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var server = Servers[Context.ConnectionId];
            server.Transport.Complete();
            await server.Transport.Completion;
            await base.OnDisconnectedAsync(exception);
        }

        public ChannelReader<OperationMessage> Connect()
        {
            var server = Servers[Context.ConnectionId];
            var hubClientTransport = (HubClientTransport) server.Transport;

            return hubClientTransport.MessageChannel.Reader;
        }

        public Task Request(OperationMessage operation)
        {
            var server = Servers[Context.ConnectionId];
            var hubClientTransport = (HubClientTransport) server.Transport;

            return hubClientTransport.ConsumeMessage(operation);
        }
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

    public class Operation
    {
        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }
}