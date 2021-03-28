namespace Tanka.GraphQL.Experimental
{
    public class OperationCoreBuilder
    {
        public static OperationExecutor BuildExecuteOperation(
            ExecuteSelectionSet executeSelectionSet
        )
        {
            return (context, _, initialValue, cancellationToken) =>
                OperationCore.ExecuteOperation(
                    context,
                    executeSelectionSet,
                    initialValue,
                    cancellationToken);
        }

        public static OperationExecutor BuildExecuteSubscription(
            CreateSourceEventStream createSourceEventStream,
            MapSourceToResponseEvent mapSourceToResponseEvent
        )
        {
            return (context, options, initialValue, cancellationToken) =>
                OperationCore.ExecuteSubscription(
                    context,
                    createSourceEventStream,
                    mapSourceToResponseEvent,
                    initialValue,
                    cancellationToken);
        }

        public static ExecuteSelectionSet BuildExecuteSelectionSet(
            CollectFields collectFields,
            ExecuteField executeField
        )
        {
            return (context, objectDefinition, objectValue, selectionSet, path, cancellationToken) =>
                OperationCore.ExecuteSelectionSet(
                    context,
                    objectDefinition,
                    objectValue,
                    selectionSet,
                    path,
                    collectFields,
                    executeField,
                    cancellationToken);
        }

        public static CollectFields BuildCollectFields(
            CoerceValue coerceValue
        )
        {
            return (context, objectDefinition, selectionSet, visitedFragments, cancellationToken) =>
                OperationCore.CollectFields(
                    context,
                    objectDefinition,
                    selectionSet,
                    coerceValue,
                    visitedFragments,
                    cancellationToken);
        }

        public static ExecuteField BuildExecuteField(
            CoerceArgumentValues coerceArgumentValues,
            ResolveFieldValue resolveFieldValue,
            CompleteValue completeValue)
        {
            return (context, objectDefinition, objectValue, fieldType, fields, path, cancellationToken) =>
                OperationCore.ExecuteField(
                    context,
                    objectDefinition,
                    objectValue,
                    fieldType,
                    fields,
                    path,
                    coerceArgumentValues,
                    resolveFieldValue,
                    completeValue,
                    cancellationToken);
        }

        public static CoerceArgumentValues BuildCoerceArgumentValues(
            CoerceValue coerceValue)
        {
            return (schema, objectDefinition, field, variableValues, cancellationToken) =>
                Coerce.CoerceArgumentValues(schema,
                    objectDefinition,
                    field,
                    coerceValue,
                    variableValues,
                    cancellationToken);
        }

        public static CoerceVariableValues BuildCoerceVariableValues(
            CoerceValue coerceValue)
        {
            return (schema, operation, variableValues, cancellationToken) =>
                Coerce.CoerceVariableValues(
                    schema,
                    operation,
                    variableValues,
                    coerceValue,
                    cancellationToken);
        }

        public static CreateSourceEventStream BuildCreateSourceEventStream(
            CollectFields collectFields,
            CoerceArgumentValues coerceArgumentValues,
            ResolveFieldEventStream resolveFieldEventStream)
        {
            return (context, initialValue, cancellationToken) =>
                OperationCore.CreateSourceEventStream(
                    context,
                    initialValue,
                    collectFields,
                    coerceArgumentValues,
                    resolveFieldEventStream,
                    cancellationToken);
        }

        public static MapSourceToResponseEvent BuildMapSourceToResponseEvent(
            ExecuteSubscriptionEvent executeSubscriptionEvent)
        {
            return (context, sourceStream, cancellationToken) =>
                OperationCore.MapSourceToResponseEvent(
                    context,
                    sourceStream,
                    executeSubscriptionEvent,
                    cancellationToken);
        }

        public static ExecuteSubscriptionEvent BuildExecuteSubscriptionEvent(
            ExecuteSelectionSet executeSelectionSet)
        {
            return (context, @event, cancellationToken) =>
                OperationCore.ExecuteSubscriptionEvent(
                    context,
                    @event,
                    executeSelectionSet,
                    cancellationToken);
        }
    }
}