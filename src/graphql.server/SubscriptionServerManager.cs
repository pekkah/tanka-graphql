using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using fugu.graphql.server.subscriptions;
using fugu.graphql.type;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.server
{
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

        public void OnConnected(string connectionId, IServerClient client)
        {
            var transport = new HubClientTransport(client);
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

        public Task Stop(string connectionId, string id)
        {
            var server = Servers[connectionId];
            var hubClientTransport = (HubClientTransport) server.Transport;

            return hubClientTransport.ConsumeMessage(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_STOP
            });
        }
    }
}