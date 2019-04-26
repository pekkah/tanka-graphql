using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using tanka.graphql.channels;
using tanka.graphql.requests;
using tanka.graphql.server.webSockets.dtos;

namespace tanka.graphql.server.webSockets
{
    public struct Subscription
    {
        public Subscription(string id, QueryStream queryStream, ChannelWriter<OperationMessage> output, CancellationTokenSource cancellationTokenSource)
        {
            QueryStream = queryStream;
            Unsubscribe = cancellationTokenSource;

            // stream results to output
            var _ = queryStream.Reader.TransformAndLinkTo(output, result => new OperationMessage()
            {
                Id = id,
                Type = MessageType.GQL_DATA,
                Payload = result,
            });
        }

        public QueryStream QueryStream { get; set; }

        public CancellationTokenSource Unsubscribe { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocol : IProtocolHandler
    {
        private readonly QueryStreamService _queryStreamService;

        protected ConcurrentDictionary<string, Subscription> Subscriptions { get; } =
            new ConcurrentDictionary<string, Subscription>();

        public GraphQLWSProtocol(QueryStreamService queryStreamService)
        {
            _queryStreamService = queryStreamService;
        }

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

        private ValueTask HandleUnknownAsync(MessageContext context)
        {
            var message = context.Message;
            return context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ERROR,
                Id = message.Id,
                Payload = new ExecutionResult
                {
                    Errors = new[]
                    {
                        new Error($"Unexpected message type {message.Type}")
                    }
                }
            });
        }

        private ValueTask HandleTerminateAsync(MessageContext context)
        {
            return default;
        }

        private ValueTask HandleStopAsync(MessageContext context)
        {
            return default;
        }

        private async ValueTask HandleStartAsync(MessageContext context)
        {
            var id = context.Message.Id;
            var payload = (OperationMessagePayload)context.Message.Payload;
            var cts = new CancellationTokenSource();
            var queryStream = await _queryStreamService.QueryAsync(new QueryRequest()
            {
                OperationName = payload.OperationName,
                Query = payload.Query,
                Variables = payload.Variables
            }, cts.Token);
            
            if (!Subscriptions.TryAdd(id, new Subscription(id, queryStream, context.Output, cts)))
            {
                await context.Output.WriteAsync(new OperationMessage
                {
                    Type = MessageType.GQL_CONNECTION_ERROR,
                    Id = id,
                    Payload = new ExecutionResult
                    {
                        Errors = new[]
                        {
                            new Error($"Subscription with {id} already exists or is not completed")
                        }
                    }
                }, CancellationToken.None);
            }
        }

        private ValueTask HandleInitAsync(MessageContext context)
        {
            return context.Output.WriteAsync(new OperationMessage
            {
                Type = MessageType.GQL_CONNECTION_ACK
            });
        }
    }
}