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
            IErrorTransformer errorTransformer,
            GraphQLDocument document,
            GraphQLOperationDefinition subscription,
            ISchema schema,
            Dictionary<string, object> coercedVariableValues,
            object initialValue)
        {
            if (errorTransformer == null) throw new ArgumentNullException(nameof(errorTransformer));
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            if (schema.Subscription == null)
                throw new GraphQLError(
                    $"Schema does not support subscriptions. Subscription type is null");

            var context = new ParallelExecutionContext(
                schema,
                document);

            try
            {
                var sourceStream = await CreateSourceEventStreamAsync(
                    context,
                    subscription,
                    coercedVariableValues,
                    initialValue).ConfigureAwait(false);

                var responseStream = MapSourceToResponseEventAsync(
                    context,
                    sourceStream,
                    subscription,
                    coercedVariableValues,
                    errorTransformer);

                return responseStream;
            }
            catch (Exception e)
            {
                context.FieldErrors.Add(e);
            }

            return new SubscriptionResult(null, null)
            {
                Errors = context.FieldErrors.SelectMany(errorTransformer.Transfrom).ToList(),
            };
        }

        public static SubscriptionResult MapSourceToResponseEventAsync(
            IExecutionContext context,
            ISubscribeResult subscribeResult,
            GraphQLOperationDefinition subscription,
            Dictionary<string, object> coercedVariableValues,
            IErrorTransformer errorTransformer)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (subscribeResult == null) throw new ArgumentNullException(nameof(subscribeResult));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));
            if (errorTransformer == null) throw new ArgumentNullException(nameof(errorTransformer));

            var executorEventBlock = new TransformBlock<object, ExecutionResult>(evnt => ExecuteSubscriptionEventAsync(
                context,
                subscription,
                coercedVariableValues,
                evnt,
                errorTransformer));

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
            IExecutionContext context,
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
            IExecutionContext context,
            GraphQLOperationDefinition subscription,
            Dictionary<string, object> coercedVariableValues,
            object evnt,
            IErrorTransformer errorTransformer)
        {
            var subscriptionType = context.Schema.Subscription;
            var selectionSet = subscription.SelectionSet;
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context,
                selectionSet,
                subscriptionType,
                evnt,
                coercedVariableValues).ConfigureAwait(false);

            return new ExecutionResult
            {
                Errors = context.FieldErrors.SelectMany(errorTransformer.Transfrom).ToList(),
                Data = data
            };
        }
    }
}