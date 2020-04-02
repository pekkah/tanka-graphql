using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Execution
{
    public static class Subscription
    {
        public static async Task<SubscriptionResult> SubscribeAsync(
            QueryContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (schema, _, operation, initialValue, coercedVariableValues) = context;

            if (schema.Subscription == null)
                throw new QueryExecutionException(
                    "Schema does not support subscriptions. Subscription type is null",
                    path: new NodePath());

            var executionContext = context.BuildExecutorContext(new ParallelExecutionStrategy());

            try
            {
                var sourceStream = await CreateSourceEventStreamAsync(
                    executionContext,
                    operation,
                    coercedVariableValues,
                    initialValue,
                    cancellationToken).ConfigureAwait(false);

                var responseStream = MapSourceToResponseEventAsync(
                    executionContext,
                    sourceStream,
                    operation,
                    coercedVariableValues,
                    context.FormatError,
                    cancellationToken);

                return responseStream;
            }
            catch (QueryExecutionException e)
            {
                executionContext.AddError(e);
            }

            return new SubscriptionResult(null)
            {
                Errors = executionContext
                    .FieldErrors
                    .Select(context.FormatError)
                    .ToList()
            };
        }

        public static SubscriptionResult MapSourceToResponseEventAsync(
            IExecutorContext context,
            ISubscriberResult subscriberResult,
            OperationDefinition subscription,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            Func<Exception, ExecutionError> formatError, 
            CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (subscriberResult == null) throw new ArgumentNullException(nameof(subscriberResult));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));
            if (formatError == null) throw new ArgumentNullException(nameof(formatError));

            var responseStream = Channel.CreateUnbounded<ExecutionResult>();
            var reader = subscriberResult.Reader;

            // execute event
            var _ = reader.TransformAndWriteTo(responseStream, item => ExecuteSubscriptionEventAsync(
                context,
                subscription,
                coercedVariableValues,
                item,
                formatError));

            return new SubscriptionResult(responseStream);
        }

        public static async Task<ISubscriberResult> CreateSourceEventStreamAsync(
            IExecutorContext context,
            OperationDefinition subscription,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            object initialValue,
            CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            cancellationToken.ThrowIfCancellationRequested();

            var schema = context.Schema;
            var subscriptionType = schema.Subscription;
            var groupedFieldSet = SelectionSets.CollectFields(
                context.Schema,
                context.Document,
                subscriptionType,
                subscription.SelectionSet,
                coercedVariableValues
            );

            var fields = groupedFieldSet.Values.First();
            var fieldName = fields.First().Name;
            var fieldSelection = fields.First();

            var coercedArgumentValues = Arguments.CoerceArgumentValues(
                schema,
                subscriptionType,
                fieldSelection,
                coercedVariableValues);

            var field = schema.GetField(subscriptionType.Name, fieldName);
            var path = new NodePath();
            var resolveContext = new ResolverContext(
                subscriptionType,
                initialValue,
                field,
                fieldSelection,
                fields,
                coercedArgumentValues,
                path,
                context);

            var subscriber = schema.GetSubscriber(subscriptionType.Name, fieldName);

            if (subscriber == null)
                throw new QueryExecutionException(
                    $"Could not subscribe. Field '{subscriptionType}:{fieldName}' does not have subscriber",
                    path);

            var subscribeResult = await subscriber(resolveContext, cancellationToken)
                .ConfigureAwait(false);

            return subscribeResult;
        }

        private static async Task<ExecutionResult> ExecuteSubscriptionEventAsync(
            IExecutorContext context,
            OperationDefinition subscription,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            object evnt,
            Func<Exception, ExecutionError> formatError)
        {
            var subscriptionType = context.Schema.Subscription;
            var selectionSet = subscription.SelectionSet;
            var path = new NodePath();
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context,
                selectionSet,
                subscriptionType,
                evnt,
                path).ConfigureAwait(false);

            var result = new ExecutionResult
            {
                Errors = context.FieldErrors.Select(formatError).ToList(),
                Data = data?.ToDictionary(kv => kv.Key, kv => kv.Value)
            };

            return result;
        }
    }
}