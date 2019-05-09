using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using tanka.graphql.channels;
using tanka.graphql.requests;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocol : IProtocolHandler
    {
        private readonly GraphQLWSProtocolOptions _options;
        private readonly IQueryStreamService _queryStreamService;
        private readonly IMessageContextAccessor _messageContextAccessor;
        private readonly ILogger<GraphQLWSProtocol> _logger;
        private readonly JsonSerializer _serializer;

        public GraphQLWSProtocol(
            IQueryStreamService queryStreamService,
            IOptions<GraphQLWSProtocolOptions> options,
            IMessageContextAccessor messageContextAccessor,
            ILogger<GraphQLWSProtocol> logger)
        {
            _queryStreamService = queryStreamService;
            _messageContextAccessor = messageContextAccessor;
            _logger = logger;
            _options = options.Value;
            _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        protected ConcurrentDictionary<string, Subscription> Subscriptions { get; } =
            new ConcurrentDictionary<string, Subscription>();

        public ValueTask Handle(MessageContext context)
        {
            _logger.LogInformation("Handling message: {id}:{type}", 
                context.Message.Id,
                context.Message.Type);

            _messageContextAccessor.Context = context;
            return context.Message.Type switch
                {
                MessageType.GQL_CONNECTION_INIT => HandleInitAsync(context),
                MessageType.GQL_START => HandleStartAsync(context),
                MessageType.GQL_STOP => HandleStopAsync(context),
                MessageType.GQL_CONNECTION_TERMINATE => HandleTerminateAsync(context),
                _ => HandleUnknownAsync(context),
                };
        }

        public Subscription GetSubscription(string id)
        {
            if (Subscriptions.TryGetValue(id, out var sub))
                return sub;

            return default;
        }

        private ValueTask HandleUnknownAsync(MessageContext context)
        {
            _logger.LogError("Unknown message received of type: {type}",
                context.Message.Type);

            var message = context.Message;
            return context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ERROR,
                Id = message.Id
            });
        }

        private ValueTask HandleTerminateAsync(MessageContext context)
        {
            _logger.LogInformation("Terminate message received");
            context.Output.TryComplete();
            return default;
        }

        private ValueTask HandleStopAsync(MessageContext context)
        {
            var id = context.Message.Id;
            var subscription = GetSubscription(id);

            if (subscription.Equals(default(Subscription)))
                return default;

            _logger.LogInformation("Stop: {id}", id);
            Subscriptions.TryRemove(id, out _);

            // unsubscribe the stream
            if (!subscription.Unsubscribe.IsCancellationRequested)
                subscription.Unsubscribe.Cancel();

            return context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_COMPLETE,
                Id = id
            }, CancellationToken.None);
        }

        private async ValueTask HandleStartAsync(MessageContext context)
        {
            var id = context.Message.Id;
            _logger.LogInformation(
                "Start: {id}",
                id);

            if (string.IsNullOrEmpty(id)) await WriteError(context, "Message.Id is required");

            if (Subscriptions.ContainsKey(id))
            {
                await WriteError(context, $"Subscription with '{id}' already exists or is not completed");
                return;
            }

            var payload = context.Message.Payload.ToObject<OperationMessageQueryPayload>(_serializer);

            var cts = new CancellationTokenSource();
            var queryStream = await _queryStreamService.QueryAsync(new QueryRequest
            {
                OperationName = payload.OperationName,
                Query = payload.Query,
                Variables = payload.Variables
            }, cts.Token);
            
            
            // There might have been another start with the id between this and the contains
            // check in the beginning. todo(pekka): refactor
            Subscriptions.TryAdd(id, new Subscription(id, queryStream, context.Output, cts));

            // stream results to output
            var _ = queryStream.Reader.TransformAndWriteTo(
                context.Output, result => new OperationMessage
                {
                    Id = id,
                    Type = MessageType.GQL_DATA,
                    Payload = JObject.FromObject(result, _serializer)
                });

            // stop on completion
            var __ = Task.Factory.StartNew(async () =>
            {
                await queryStream.Reader.Completion;
                await HandleStopAsync(context);
            }, CancellationToken.None);
        }

        private async Task WriteError(MessageContext context, string errorMessage)
        {
            _logger.LogError("{message}", errorMessage);
            await context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ERROR,
                Id = context.Message.Id,
                Payload = JObject.FromObject(new ExecutionResult
                {
                    Errors = new[]
                    {
                        new Error(errorMessage)
                    }
                }, JsonSerializer.Create())
            }, CancellationToken.None);
        }

        private async ValueTask HandleInitAsync(MessageContext context)
        {
            _logger.LogInformation("Init");
            await _options.AcceptAsync(context);
        }
    }
}