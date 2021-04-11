using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;

namespace Tanka.GraphQL.Experimental.Core
{
    public class OperationCoreBuilder
    {
        public static OperationExecutor BuildExecuteOperation()
        {
            return (context, _, initialValue, cancellationToken) =>
                OperationCore.ExecuteOperation(
                    context,
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
            return (schema, objectDefinition, field, variableValues, cancellationToken) => Coerce.CoerceArgumentValues(
                schema,
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

        public static ExecuteSubscriptionEvent BuildExecuteSubscriptionEvent()
        {
            return OperationCore.ExecuteSubscriptionEvent;
        }

        public static CompleteValue BuildCompleteValue(
            SerializeValue serializeValue)
        {
            return (context, fieldType, fields, resolvedValue, resolveAbstractType, path, cancellationToken) =>
                OperationCore.CompleteValue(
                    context,
                    fieldType,
                    fields,
                    resolvedValue,
                    path,
                    serializeValue,
                    resolveAbstractType,
                    cancellationToken);
        }

        public static CoerceValue BuildCoerceValue(
            IReadOnlyDictionary<string, CoerceValue> valueConverter)
        {
            return (schema, value, type) =>
                Coerce.CoerceValue(schema, value, type, valueConverter);
        }

        public static ValidateOperation BuildValidator()
        {
            return (context, options, cancellationToken) => Task.CompletedTask;
        }

        public static SerializeValue BuildSerializeValue(IReadOnlyDictionary<string, SerializeValue> valueSerializers)
        {
            return (schema, definition, value) =>
                Serialize.SerializeValue(schema, value, definition, valueSerializers);
        }
    }
}