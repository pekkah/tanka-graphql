using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using tanka.graphql.channels;
using tanka.graphql.requests;
using tanka.graphql.resolvers;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocol : IProtocolHandler
    {
        private readonly ILogger<GraphQLWSProtocol> _logger;
        private readonly IMessageContextAccessor _messageContextAccessor;
        private readonly GraphQLWSProtocolOptions _options;
        private readonly IQueryStreamService _queryStreamService;
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

        private async ValueTask HandleStopAsync(MessageContext context)
        {
            var id = context.Message.Id;
            var subscription = GetSubscription(id);

            if (subscription.Equals(default(Subscription)))
                return;

            // unsubscribe the stream
            if (!subscription.Unsubscribe.IsCancellationRequested)
                subscription.Unsubscribe.Cancel();

            Subscriptions.TryRemove(id, out _);

            await subscription.QueryStream.Reader.Completion;
        }

        private async ValueTask HandleStartAsync(MessageContext context)
        {
            var id = context.Message.Id;
            _logger.LogInformation(
                "Start: {id}",
                id);

            if (string.IsNullOrEmpty(id))
            {
                await WriteError(context, "Message.Id is required");
                return;
            }

            if (Subscriptions.ContainsKey(id))
            {
                await WriteError(context, $"Subscription with '{id}' already exists or is not completed");
                return;
            }

            var payload = context
                .Message
                .Payload
                .ToObject<OperationMessageQueryPayload>(_serializer);

            using var logScope = _logger.BeginScope("Query: '{operationName}'", payload.OperationName);

            var document = Parser.ParseDocument(payload.Query);
            var unsubscribeSource = new CancellationTokenSource();
            var queryStream = await _queryStreamService.QueryAsync(new Query
            {
                OperationName = payload.OperationName,
                Document = document,
                Variables = payload.Variables
            }, unsubscribeSource.Token);

            // transform to output
            // stream results to output
            var _ = queryStream.Reader.TransformAndWriteTo(
                context.Output, result => new OperationMessage
                {
                    Id = id,
                    Type = MessageType.GQL_DATA,
                    Payload = JObject.FromObject(result, _serializer)
                });

            // has mutation or query
            var hasMutationOrQuery = document.Definitions.OfType<GraphQLOperationDefinition>()
                .Any(op => op.Operation != OperationType.Subscription);

            if (hasMutationOrQuery)
            {
                // no need to setup subscription as query or mutation will finish 
                // after execution
                await ExecuteQueryOrMutationStream(context, queryStream);
                return;
            }

            // execute subscription
            await ExecuteSubscriptionStream(context, queryStream, unsubscribeSource);
        }

        private async ValueTask ExecuteSubscriptionStream(
            MessageContext context, 
            QueryStream queryStream,
            CancellationTokenSource unsubscribeSource)
        {
            _logger.LogInformation("Executing subscription stream");
            var id = context.Message.Id;
            // There might have been another start with the id between this and the contains
            // check in the beginning. todo(pekka): refactor
            var sub = new Subscription(id, queryStream, context.Output, unsubscribeSource);
            if(!Subscriptions.TryAdd(id, sub))
            {
                sub.Unsubscribe.Cancel();
                await WriteError(context, $"Subscription with id '{id}' already exists");
                return;
            }

            var _ = Task.Run(async () =>
            {
                await queryStream.Reader.Completion;
                _logger.LogInformation("Stop: '{id}'", id);
                Subscriptions.TryRemove(id, out sub);
                await context.Output.WriteAsync(new OperationMessage
                {
                    Type = MessageType.GQL_COMPLETE,
                    Id = id
                }, CancellationToken.None);
                _logger.LogInformation("Query '{id}' completed", id);
            });
        }

        private ValueTask ExecuteQueryOrMutationStream(MessageContext context, QueryStream queryStream)
        {
            _logger.LogInformation("Executing query or mutation stream");
            var id = context.Message.Id;
            var __ = queryStream.Reader.Completion.ContinueWith(async result =>
            {
                _logger.LogInformation("Stop: '{id}'", id);
                await context.Output.WriteAsync(new OperationMessage
                {
                    Type = MessageType.GQL_COMPLETE,
                    Id = id
                }, CancellationToken.None);
                _logger.LogInformation("Query '{id}' completed", id);
            }, TaskContinuationOptions.LongRunning);

            return default;
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
                }, _serializer)
            }, CancellationToken.None);
        }

        private async ValueTask HandleInitAsync(MessageContext context)
        {
            _logger.LogInformation("Init: {payload}", context
                .Message
                .Payload
                ?.ToString());

            await _options.AcceptAsync(context);
        }
    }
}