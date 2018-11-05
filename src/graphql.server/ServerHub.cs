using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.type;
using GraphQLParser.AST;
using Microsoft.AspNetCore.SignalR;

namespace fugu.graphql.server
{
    public class ServerClients
    {
        private readonly ConcurrentDictionary<string, QueryManager> _clients =
            new ConcurrentDictionary<string, QueryManager>();

        private readonly ISchema _schema;

        public ServerClients(ISchema schema)
        {
            _schema = schema;
        }

        public Task OnConnectedAsync(HubCallerContext context)
        {
            _clients[context.ConnectionId] = new QueryManager(_schema);
            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync(HubCallerContext context, Exception exception)
        {
            if (_clients.TryGetValue(context.ConnectionId, out var queryManager)) return queryManager.CloseAllAsync();

            return Task.CompletedTask;
        }

        public Task<QueryStream> QueryAsync(HubCallerContext context, QueryRequest query,
            CancellationToken cancellationToken)
        {
            if (!_clients.TryGetValue(context.ConnectionId, out var queryManager))
                throw new InvalidOperationException($"No QueryManager for connection '{context.ConnectionId}'");

            return queryManager.QueryAsync(query, cancellationToken);
        }
    }

    public class ServerHub : Hub
    {
        private readonly ServerClients _clients;

        public ServerHub(ServerClients clients)
        {
            _clients = clients;
        }

        public override Task OnConnectedAsync()
        {
            return _clients.OnConnectedAsync(Context);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return _clients.OnDisconnectedAsync(Context, exception);
        }

        [HubMethodName("query")]
        public async Task<ChannelReader<ExecutionResult>> QueryAsync(QueryRequest query,
            CancellationToken cancellationToken)
        {
            var queryResult = await _clients.QueryAsync(Context, query, cancellationToken);
            var channel = queryResult.Channel;
            return channel.Reader;
        }
    }

    public class QueryRequest
    {
        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }

    public class QueryManager
    {
        private readonly ISchema _schema;

        public QueryManager(ISchema schema)
        {
            _schema = schema;
        }

        public async Task CloseAllAsync()
        {
        }

        public async Task<QueryStream> QueryAsync(QueryRequest query, CancellationToken cancellationToken)
        {
            var document = Parser.ParseDocument(query.Query);

            // is subscription
            if (document.Definitions.OfType<GraphQLOperationDefinition>()
                .Any(op => op.Operation == OperationType.Subscription))
                return await SubscribeAsync(
                    document,
                    query.OperationName,
                    query.Variables,
                    query.Extensions,
                    cancellationToken);


            // is query or mutation
            return await ExecuteAsync(
                document,
                query.OperationName,
                query.Variables,
                query.Extensions,
                cancellationToken);
        }

        private async Task<QueryStream> ExecuteAsync(GraphQLDocument document,
            string operationName,
            Dictionary<string, object> variables,
            Dictionary<string, object> extensions,
            CancellationToken cancellationToken)
        {
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document = document,
                OperationName = operationName,
                VariableValues = variables,
                InitialValue = null
            });

            var channel = Channel.CreateBounded<ExecutionResult>(1);
            await channel.Writer.WriteAsync(result, cancellationToken);

            return new QueryStream(channel);
        }

        private async Task<QueryStream> SubscribeAsync(GraphQLDocument document,
            string operationName,
            Dictionary<string, object> variables,
            Dictionary<string, object> extensions,
            CancellationToken cancellationToken)
        {
            var result = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document = document,
                OperationName = operationName,
                VariableValues = variables,
                InitialValue = null
            });

            cancellationToken.Register(async () => await result.UnsubscribeAsync());

            var channel = Channel.CreateUnbounded<ExecutionResult>();
            var writer = new ActionBlock<ExecutionResult>(
                executionResult => channel.Writer.WriteAsync(executionResult, cancellationToken),
                new ExecutionDataflowBlockOptions
                {
                    EnsureOrdered = true
                });

            result.Source.LinkTo(writer, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return new QueryStream(channel);
        }
    }

    public class QueryStream
    {
        public QueryStream(Channel<ExecutionResult> channel)
        {
            Channel = channel;
        }

        public Channel<ExecutionResult> Channel { get; }
    }
}