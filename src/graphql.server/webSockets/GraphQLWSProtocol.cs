using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using tanka.graphql.channels;
using tanka.graphql.requests;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocolOptions
    {
        private static readonly Task<bool> True = Task.FromResult(true);

        /// <summary>
        ///     Method called when initialize message is received from client to validate
        ///     the connectionParams
        /// </summary>
        /// <returns>true if connection accepted; otherwise false</returns>
        public Func<MessageContext, Task<bool>> Initialize { get; set; } = context => True;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocol : IProtocolHandler
    {
        private readonly ConcurrentQueue<MessageContext> _initializationQueue = new ConcurrentQueue<MessageContext>();
        private readonly GraphQLWSProtocolOptions _options;
        private readonly IQueryStreamService _queryStreamService;
        private readonly JsonSerializer _serializer;
        private volatile bool _isInitialized;


        public GraphQLWSProtocol(IQueryStreamService queryStreamService, GraphQLWSProtocolOptions options)
        {
            _queryStreamService = queryStreamService;
            _options = options;
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
            if (!_isInitialized)
                return context.Message.Type switch
                    {
                    MessageType.GQL_CONNECTION_INIT => HandleInitAsync(context),
                    _ => QueueMessage(context),
                    };

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

        private ValueTask QueueMessage(MessageContext context)
        {
            _initializationQueue.Enqueue(context);
            return default;
        }

        private ValueTask HandleUnknownAsync(MessageContext context)
        {
            var message = context.Message;
            return context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ERROR,
                Id = message.Id
            });
        }

        private ValueTask HandleTerminateAsync(MessageContext context)
        {
            context.Output.TryComplete();
            return default;
        }

        private ValueTask HandleStopAsync(MessageContext context)
        {
            var id = context.Message.Id;
            var subscription = GetSubscription(id);

            if (subscription.Equals(default))
                return default;

            Subscriptions.TryRemove(id, out _);

            // unsubscribe the stream
            if (!subscription.Unsubscribe.IsCancellationRequested) subscription.Unsubscribe.Cancel();

            // write complete
            return context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_COMPLETE,
                Id = id
            });
        }

        private async ValueTask HandleStartAsync(MessageContext context)
        {
            var id = context.Message.Id;

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

            // stream results to output
            var _ = queryStream.Reader.TransformAndWriteTo(
                context.Output, result => new OperationMessage
                {
                    Id = id,
                    Type = MessageType.GQL_DATA,
                    Payload = JObject.FromObject(result, _serializer)
                });

            // There might have been another start with the id between this and the contains
            // check in the beginning. todo(pekka): refactor
            Subscriptions.TryAdd(id, new Subscription(id, queryStream, context.Output, cts));
        }

        private static async Task WriteError(MessageContext context, string errorMessage)
        {
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
            var accepted = await _options.Initialize(context);

            if (accepted)
            {
                await FlushInitializationQueue();
                _isInitialized = true;
                await context.Output.WriteAsync(new OperationMessage
                {
                    Type = MessageType.GQL_CONNECTION_ACK
                });
            }
        }

        private async ValueTask FlushInitializationQueue()
        {
            while (_initializationQueue.TryDequeue(out var messageContext)) await Handle(messageContext);
        }
    }
}