using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly JsonSerializer _serializer;

        public GraphQLWSProtocol(
            IQueryStreamService queryStreamService, 
            IOptionsMonitor<GraphQLWSProtocolOptions> options)
        {
            _queryStreamService = queryStreamService;
            _options = options.CurrentValue;
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
                await context.Output.WriteAsync(new OperationMessage
                {
                    Type = MessageType.GQL_CONNECTION_ACK
                });
            }
        }
    }
}