using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Microsoft.Extensions.Logging;
using tanka.graphql.execution;
using tanka.graphql.language;
using tanka.graphql.type;
using tanka.graphql.validation;

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
            CancellationToken cancellationToken = default)
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
                    logger);

                if (!validationResult.IsValid)
                    return new ExecutionResult
                    {
                        Errors = validationResult.Errors.Select(e => e.ToError())
                            .ToList(),
                        Extensions = validationResult.Extensions.ToDictionary(kv => kv.Key, kv => kv.Value)
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

                foreach (var validationExtension in validationResult.Extensions)
                {
                    executionResult.AddExtension(validationExtension.Key, validationExtension.Value);
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
        /// <param name="unsubscribe">Unsubscribe</param>
        /// <returns></returns>
        public static async Task<SubscriptionResult> SubscribeAsync(
            ExecutionOptions options,
            CancellationToken unsubscribe)
        {
            if (!unsubscribe.CanBeCanceled)
                throw new InvalidOperationException("Unsubscribe token must be cancelable");

            var extensions = new Extensions(Enumerable.Empty<IExtension>());
            await extensions.BeginExecuteAsync(options);

            var logger = options.LoggerFactory.CreateLogger(typeof(Executor).FullName);

            using (logger.Begin(options.OperationName))
            {
                var (queryContext, validationResult) = await BuildQueryContextAsync(
                    options,
                    extensions,
                    logger);

                if (!validationResult.IsValid)
                    return new SubscriptionResult
                    {
                        Errors = validationResult.Errors.Select(e => e.ToError())
                            .ToList(),
                        Extensions = validationResult.Extensions.ToDictionary(kv => kv.Key, kv => kv.Value)
                    };

                SubscriptionResult subscriptionResult;
                switch (queryContext.OperationDefinition.Operation)
                {
                    case OperationType.Subscription:
                        subscriptionResult = await Subscription.SubscribeAsync(
                            queryContext, 
                            unsubscribe).ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Operation type {queryContext.OperationDefinition.Operation} not supported. Did you mean to use {nameof(ExecuteAsync)}?");
                }

                foreach (var validationExtension in validationResult.Extensions)
                {
                    subscriptionResult.AddExtension(validationExtension.Key, validationExtension.Value);
                }

                logger.ExecutionResult(subscriptionResult);
                return subscriptionResult;
            }
        }

        private static async Task<(QueryContext queryContext, ValidationResult validationResult)>
            BuildQueryContextAsync(ExecutionOptions options,
                Extensions extensions,
                ILogger logger)
        {
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

            logger.Validate(options.Validate != null);
            var validationResult = ValidationResult.Success;
            if (options.Validate != null)
            {
                await extensions.BeginValidationAsync();
                validationResult = await options.Validate(
                    options.Schema,
                    document,
                    coercedVariableValues);

                logger.ValidationResult(validationResult);
                await extensions.EndValidationAsync(validationResult);
            }

            return (queryContext, validationResult);
        }
    }
}