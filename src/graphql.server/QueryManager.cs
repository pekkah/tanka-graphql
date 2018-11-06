﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.server.utilities;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.server
{
    public class QueryManager
    {
        private readonly ISchema _schema;

        public QueryManager(ISchema schema)
        {
            _schema = schema;
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
            channel.Writer.TryComplete();

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

            var channel = Channel.CreateUnbounded<ExecutionResult>();

#pragma warning disable 4014
            // ReSharper disable once MethodSupportsCancellation
            Task.Run(async () =>
#pragma warning restore 4014
            {
                await cancellationToken.WhenCancelled();
                await result.UnsubscribeAsync();
                channel.Writer.TryComplete();
            });

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

            var stream = new QueryStream(channel);
            return stream;
        }
    }
}