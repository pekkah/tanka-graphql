using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace tanka.graphql.server
{
    public class QueryStreamService
    {
        private readonly List<IExtension> _extensions;
        private readonly ILogger<QueryStreamService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptionsMonitor<QueryStreamHubOptions> _optionsMonitor;

        public QueryStreamService(
            IOptionsMonitor<QueryStreamHubOptions> optionsMonitor,
            ILoggerFactory loggerFactory,
            IEnumerable<IExtension> extensions)
        {
            _optionsMonitor = optionsMonitor;
            _loggerFactory = loggerFactory;
            _extensions = extensions.ToList();
            _logger = loggerFactory.CreateLogger<QueryStreamService>();
        }

        public async Task<QueryStream> QueryAsync(
            QueryRequest query,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.Query(query);
                var serviceOptions = _optionsMonitor.CurrentValue;
                var document = await Parser.ParseDocumentAsync(query.Query);
                var executionOptions = new ExecutionOptions
                {
                    Schema = serviceOptions.Schema,
                    Document = document,
                    OperationName = query.OperationName,
                    VariableValues = query.Variables,
                    InitialValue = null,
                    LoggerFactory = _loggerFactory,
                    Extensions = _extensions
                };

                // is subscription
                if (document.Definitions.OfType<GraphQLOperationDefinition>()
                    .Any(op => op.Operation == OperationType.Subscription))
                    return await SubscribeAsync(
                        executionOptions,
                        cancellationToken);


                // is query or mutation
                return await ExecuteAsync(
                    executionOptions,
                    cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to execute query '{query.Query}'. Error. '{e}'");
                var channel = Channel.CreateBounded<ExecutionResult>(1);
                channel.Writer.TryComplete(e);
                return new QueryStream(channel);
            }
        }

        private async Task<QueryStream> ExecuteAsync(
            ExecutionOptions options,
            CancellationToken cancellationToken)
        {
            var result = await Executor.ExecuteAsync(options);
            var channel = Channel.CreateBounded<ExecutionResult>(1);

            await channel.Writer.WriteAsync(result, cancellationToken);
            channel.Writer.TryComplete();

            _logger.Executed(options.OperationName, options.VariableValues, null);
            return new QueryStream(channel);
        }

        private async Task<QueryStream> SubscribeAsync(
            ExecutionOptions options,
            CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                throw new InvalidOperationException(
                    "Invalid cancellation token. To unsubscribe the provided cancellation token must be cancellable.");

            var unsubscribeSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var result = await Executor.SubscribeAsync(options, unsubscribeSource.Token);
            _logger.Subscribed(options.OperationName, options.VariableValues, null);

            unsubscribeSource.Token.Register(() =>
            {
                _logger?.Unsubscribed(
                    options.OperationName,
                    options.VariableValues,
                    null);
            });

            if (result.Errors != null)
            {
                var channel = Channel.CreateBounded<ExecutionResult>(1);
                await channel.Writer.WriteAsync(new ExecutionResult
                {
                    Errors = result.Errors,
                    Extensions = result.Extensions
                }, CancellationToken.None);

                channel.Writer.TryComplete();

                // unsubscribe
                unsubscribeSource.Cancel();

                return new QueryStream(channel.Reader);
            }

            var stream = new QueryStream(result.Source);
            return stream;
        }
    }
}