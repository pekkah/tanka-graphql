using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental
{
    public partial class OperationCore
    {
        public static IAsyncEnumerable<OperationResult> ExecuteSubscription(
            OperationContext context,
            CreateSourceEventStream createSourceEventStream,
            MapSourceToResponseEvent mapSourceToResponseEvent,
            object? initialValue,
            CancellationToken cancellationToken)
        {
            var sourceStream = createSourceEventStream(
                context,
                initialValue,
                cancellationToken);

            var responseStream = mapSourceToResponseEvent(
                context,
                sourceStream,
                cancellationToken);

            return responseStream;
        }

        public static async IAsyncEnumerable<object?> CreateSourceEventStream(
            OperationContext context,
            object? initialValue,
            CollectFields collectFields,
            CoerceArgumentValues coerceArgumentValues,
            ResolveFieldEventStream resolveFieldEventStream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var subscriptionType = context.Schema.Subscription;

            if (subscriptionType == null)
                throw new InvalidOperationException(
                    "Schema does not support subscriptions. No subscription root provided.");

            var groupedFieldSet = collectFields(
                context,
                subscriptionType,
                context.Operation.SelectionSet,
                cancellationToken: cancellationToken);

            if (groupedFieldSet.Count != 1)
                //todo: throw query error
                throw new InvalidOperationException(
                    "Selections for subscription must include exactly one entry");

            var (fieldName, fields) = groupedFieldSet.First();
            var field = fields.First();
            var argumentValues = await coerceArgumentValues(
                context.Schema,
                subscriptionType,
                field,
                context.CoercedVariableValues,
                cancellationToken);

            var fieldStream = resolveFieldEventStream(
                context,
                subscriptionType,
                initialValue,
                fieldName,
                argumentValues,
                cancellationToken);

            await foreach (var ev in fieldStream.WithCancellation(cancellationToken)) yield return ev;
        }

        public static async IAsyncEnumerable<OperationResult> MapSourceToResponseEvent(
            OperationContext context,
            IAsyncEnumerable<object?> sourceStream,
            ExecuteSubscriptionEvent executeSubscriptionEvent,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var ev in sourceStream.WithCancellation(cancellationToken))
            {
                var response = await executeSubscriptionEvent(
                    context,
                    ev,
                    cancellationToken);

                yield return response;
            }
        }

        public static async Task<OperationResult> ExecuteSubscriptionEvent(
            OperationContext context,
            object? @event,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var subscriptionType = context.Schema.Subscription;

            if (subscriptionType == null)
                throw new InvalidOperationException(
                    "Schema does not support subscriptions. No subscription root provided.");

            var selectionSet = context.Operation.SelectionSet;
            var result = await context.ExecuteSelectionSet(
                context,
                subscriptionType,
                @event,
                selectionSet,
                new NodePath(),
                cancellationToken);

            return new OperationResult
            {
                Data = result.Data,
                Errors = result.Errors
            };
        }
    }
}