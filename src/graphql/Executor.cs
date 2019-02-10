using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.execution;
using tanka.graphql.type;
using tanka.graphql.validation;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;

namespace tanka.graphql
{
    /// <summary>
    ///     Execute queries, mutations and subscriptions
    /// </summary>
    public static class Executor
    {
        /// <summary>
        ///     Execute query or mutation
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ExecutionResult> ExecuteAsync(
            ExecutionOptions options, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var extensions = new Extensions(options.Extensions);
            await extensions.BeginExecuteAsync(options);
            var logger = options.LoggerFactory.CreateLogger(typeof(Executor).FullName);

            using (logger.Begin(options.OperationName))
            {
                var (queryContext, validationResult) = await BuildQueryContextAsync(
                    options, 
                    extensions, 
                    logger,
                    cancellationToken);

                if (!validationResult.IsValid)
                    return new ExecutionResult
                    {
                        Errors = validationResult.Errors.Select(e => new Error(e.Message))
                    };

                ExecutionResult executionResult;
                switch (queryContext.OperationDefinition.Operation)
                {
                    case OperationType.Query:
                        executionResult = await Query.ExecuteQueryAsync(queryContext).ConfigureAwait(false);
                        break;
                    case OperationType.Mutation:
                        executionResult = await Mutation.ExecuteMutationAsync(queryContext).ConfigureAwait(false);
                        break;
                    case OperationType.Subscription:
                        throw new InvalidOperationException($"Use {nameof(SubscribeAsync)}");
                    default:
                        throw new InvalidOperationException(
                            $"Operation type {queryContext.OperationDefinition.Operation} not supported.");
                }

                logger.ExecutionResult(executionResult);
                await extensions.EndExecuteAsync(executionResult);
                return executionResult;
            }
        }

        /// <summary>
        ///     Execute subscription
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken">Unsubscribe</param>
        /// <returns></returns>
        public static async Task<SubscriptionResult> SubscribeAsync(
            ExecutionOptions options, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var extensions = new Extensions(options.Extensions);
            await extensions.BeginExecuteAsync(options);
            cancellationToken.ThrowIfCancellationRequested();

            var logger = options.LoggerFactory.CreateLogger(typeof(Executor).FullName);

            using (logger.Begin(options.OperationName))
            {
                var (queryContext, validationResult) = await BuildQueryContextAsync(
                    options, 
                    extensions, 
                    logger,
                    cancellationToken);

                if (!validationResult.IsValid)
                    return new SubscriptionResult
                    {
                        Errors = validationResult.Errors.Select(e => new Error(e.Message))
                    };

                switch (queryContext.OperationDefinition.Operation)
                {
                    case OperationType.Subscription:
                        return await Subscription.SubscribeAsync(queryContext, cancellationToken).ConfigureAwait(false);
                    default:
                        throw new InvalidOperationException(
                            $"Operation type {queryContext.OperationDefinition.Operation} not supported. Did you mean to use {nameof(ExecuteAsync)}?");
                }
            }
        }

        private static async Task<(QueryContext queryContext, ValidationResult validationResult)>
            BuildQueryContextAsync(ExecutionOptions options,
                Extensions extensions,
                ILogger logger, 
                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            /* Schema should be always initialized
            if (!options.Schema.IsInitialized)
            {
                logger.SchemaNotInitialized();
                await options.Schema.InitializeAsync().ConfigureAwait(false);
            }*/

            await extensions.BeginParseDocumentAsync();
            var document = options.Document;
            await extensions.EndParseDocumentAsync(document);

            var operation = Operations.GetOperation(document, options.OperationName);
            logger.Operation(operation);

            var coercedVariableValues = Variables.CoerceVariableValues(
                options.Schema,
                operation,
                options.VariableValues);

            var queryContext = new QueryContext(
                options.FormatError,
                document,
                operation,
                options.Schema,
                coercedVariableValues,
                options.InitialValue,
                extensions);

            logger.Validate(options.Validate);
            var validationResult = new ValidationResult();
            if (options.Validate)
            {
                await extensions.BeginValidationAsync();
                validationResult = await Validator.ValidateAsync(
                    options.Schema,
                    document,
                    coercedVariableValues).ConfigureAwait(false);

                logger.ValidationResult(validationResult);

                await extensions.EndValidationAsync(validationResult);
            }

            return (queryContext, validationResult);
        }
    }
}