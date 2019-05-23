﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.channels;
using tanka.graphql.error;
using tanka.graphql.resolvers;

namespace tanka.graphql.execution
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
                throw new GraphQLError(
                    "Schema does not support subscriptions. Subscription type is null");

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
            catch (GraphQLError e)
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
            ISubscribeResult subscribeResult,
            GraphQLOperationDefinition subscription,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            Func<Exception, Error> formatError, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (subscribeResult == null) throw new ArgumentNullException(nameof(subscribeResult));
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));
            if (formatError == null) throw new ArgumentNullException(nameof(formatError));

            var responseStream = Channel.CreateUnbounded<ExecutionResult>();
            var reader = subscribeResult.Reader;

            // execute event
            var _ = reader.TransformAndLinkTo(responseStream, item => ExecuteSubscriptionEventAsync(
                context,
                subscription,
                coercedVariableValues,
                item,
                formatError));

            return new SubscriptionResult(responseStream);
        }

        public static async Task<ISubscribeResult> CreateSourceEventStreamAsync(
            IExecutorContext context,
            GraphQLOperationDefinition subscription,
            IReadOnlyDictionary<string, object> coercedVariableValues,
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
            var fieldName = fields.First().Name.Value;
            var fieldSelection = fields.First();

            var coercedArgumentValues = Arguments.CoerceArgumentValues(
                schema,
                subscriptionType,
                fieldSelection,
                coercedVariableValues);

            var field = schema.GetField(subscriptionType.Name, fieldName);
            var path = new NodePath();
            var resolveContext = new ResolverContext(
                schema,
                subscriptionType,
                initialValue,
                field,
                fieldSelection,
                coercedArgumentValues,
                path,
                context);

            var subscriber = schema.GetSubscriber(subscriptionType.Name, fieldName);

            if (subscriber == null)
                throw new GraphQLError(
                    $"Could not subscribe. Field '{subscriptionType}:{fieldName}' does not have subscriber");

            var subscribeResult = await subscriber(resolveContext, cancellationToken)
                .ConfigureAwait(false);

            return subscribeResult;
        }

        private static async Task<ExecutionResult> ExecuteSubscriptionEventAsync(
            IExecutorContext context,
            GraphQLOperationDefinition subscription,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            object evnt,
            Func<Exception, Error> formatError)
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

            var result = new ExecutionResult
            {
                Errors = context.FieldErrors.Select(formatError).ToList(),
                Data = data
            };

            return result;
        }
    }
}