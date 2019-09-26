using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;

namespace Tanka.GraphQL
{
    public static partial class Executor
    {
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

            var extensions = new ExtensionsRunner(Enumerable.Empty<IExecutorExtension>());
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

                //todo: this looks ugly
                if (validationResult.Extensions != null)
                    foreach (var validationExtension in validationResult.Extensions)
                        subscriptionResult.AddExtension(validationExtension.Key, validationExtension.Value);

                logger.ExecutionResult(subscriptionResult);
                return subscriptionResult;
            }
        }
    }
}