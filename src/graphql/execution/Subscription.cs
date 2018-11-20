using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.error;
using fugu.graphql.resolvers;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Subscription
    {
        public static async Task<SubscriptionResult> SubscribeAsync(
            QueryContext context)
        {
            var (schema, document, operation, initialValue, coercedVariableValues) = context;

            if (schema.Subscription == null)
                throw new GraphQLError(
                    $"Schema does not support subscriptions. Subscription type is null");

            var executionContext = new ExecutorContext(
                schema, 
                document,
                new ParallelExecutionStrategy());

            try
            {
                var sourceStream = await CreateSourceEventStreamAsync(
                    executionContext,
                    operation,
                    coercedVariableValues,
                    initialValue).ConfigureAwait(false);

                var responseStream = MapSourceToResponseEventAsync(
                    executionContext,
                    sourceStream,
                    operation,
                    coercedVariableValues,
                    context.FormatError);

                return responseStream;
            }
            catch (GraphQLError e)
            {
                executionContext.AddError(e);
            }

            return new SubscriptionResult(null, null)
            {
                Errors = executionContext
                    .FieldErrors
                    .Select(context.FormatError)
                    .ToList(),
            };
        }

        public static SubscriptionResult MapSourceToResponseEventAsync(
            IExecutorContext context,
            ISubscribeResult subscribeResult,
            GraphQLOperationDefinition subscription,
            Dictionary<string, object> coercedVariableValues,
            Func<GraphQLError, Error> formatError)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (subscribeResult == null) throw new ArgumentNullException(nameof(subscribeResult));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));
            if (formatError == null) throw new ArgumentNullException(nameof(formatError));

            var executorEventBlock = new TransformBlock<object, ExecutionResult>(evnt => ExecuteSubscriptionEventAsync(
                context,
                subscription,
                coercedVariableValues,
                evnt,
                formatError));

            var sourceStream = subscribeResult.Reader;
            sourceStream.LinkTo(executorEventBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            var responseStream = new BufferBlock<ExecutionResult>();
            executorEventBlock.LinkTo(responseStream, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return new SubscriptionResult(responseStream, subscribeResult.UnsubscribeAsync);
        }

        public static async Task<ISubscribeResult> CreateSourceEventStreamAsync(
            IExecutorContext context,
            GraphQLOperationDefinition subscription,
            Dictionary<string, object> coercedVariableValues,
            object initialValue)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

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
            var fieldName = fields.First().Name.Value;
            var fieldSelection = fields.First();

            var coercedArgumentValues = Arguments.CoerceArgumentValues(
                subscriptionType,
                fieldSelection,
                coercedVariableValues);

            var field = subscriptionType.GetField(fieldName);
            var resolveContext = new ResolverContext(
                subscriptionType,
                initialValue,
                field,
                fieldSelection,
                coercedArgumentValues);

            var subscriber = field.Subscribe;

            if (subscriber == null)
                throw new GraphQLError(
                    $"Could not subscribe. Field '{subscriptionType}:{fieldName}' does not have subscriber");

            var subscribeResult = await subscriber(resolveContext)
                .ConfigureAwait(false);

            return subscribeResult;
        }

        private static async Task<ExecutionResult> ExecuteSubscriptionEventAsync(
            IExecutorContext context,
            GraphQLOperationDefinition subscription,
            Dictionary<string, object> coercedVariableValues,
            object evnt,
            Func<GraphQLError, Error> formatError)
        {
            var subscriptionType = context.Schema.Subscription;
            var selectionSet = subscription.SelectionSet;
            var path = new NodePath();
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context,
                selectionSet,
                subscriptionType,
                evnt,
                coercedVariableValues,
                path).ConfigureAwait(false);

            return new ExecutionResult
            {
                Errors = context.FieldErrors.Select(formatError).ToList(),
                Data = data
            };
        }
    }
}