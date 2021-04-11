using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Core;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public static class Request
    {
        public static ExecuteRequest UseExecuteRequest(
            ExecutableSchema schema,
            ResolveFieldValue resolve,
            ResolveFieldEventStream subscribe,
            IReadOnlyDictionary<string, CoerceValue> valueConverters,
            IReadOnlyDictionary<string, SerializeValue> valueSerializers)
        {
            var executeRequest = RequestCoreBuilder.BuildExecute(
                UseCreateOperationContext(resolve, subscribe, valueConverters, valueSerializers)
            );

            return (requestOptions, initialValue, cancellationToken) =>
            {
                var executeOptions = requestOptions with
                {
                    Schema = schema
                };

                return executeRequest(executeOptions, initialValue, cancellationToken);
            };
        }

        public static ExecuteRequestSingle UseExecuteRequestSingle(
            ExecutableSchema schema,
            ResolveFieldValue resolve,
            ResolveFieldEventStream subscribe,
            IReadOnlyDictionary<string, CoerceValue> valueConverters,
            IReadOnlyDictionary<string, SerializeValue> valueSerializers)
        {
            var executeRequest = UseExecuteRequest(schema, resolve, subscribe, valueConverters, valueSerializers);
            return RequestCoreBuilder.BuildExecuteSingle(executeRequest);
        }


        public static CreateOperationContext UseCreateOperationContext(
            ResolveFieldValue resolve,
            ResolveFieldEventStream subscribe,
            IReadOnlyDictionary<string, CoerceValue> valueConverters,
            IReadOnlyDictionary<string, SerializeValue> valueSerializers)
        {
            var coerceValue = OperationCoreBuilder.BuildCoerceValue(valueConverters);
            var serializeValue = OperationCoreBuilder.BuildSerializeValue(valueSerializers);
            return RequestCoreBuilder.BuildCreateOperationContext(
                UseOperationSelector(),
                OperationCoreBuilder.BuildCoerceVariableValues(coerceValue),
                OperationCoreBuilder.BuildValidator(),
                UseOperationExecutorSelector(subscribe, coerceValue),
                UseExecuteSelectionSetSelector(resolve, coerceValue, serializeValue),
                coerceValue
            );
        }

        private static ExecuteSelectionSetSelector UseExecuteSelectionSetSelector(
            ResolveFieldValue resolve,
            CoerceValue coerceValue,
            SerializeValue serializeValue)
        {
            var collectFields = OperationCoreBuilder.BuildCollectFields(coerceValue);
            var coerceArgumentValues = OperationCoreBuilder.BuildCoerceArgumentValues(coerceValue);

            var completeValue = OperationCoreBuilder.BuildCompleteValue(
                serializeValue
            );

            var executeSelectionSet = OperationCoreBuilder.BuildExecuteSelectionSet(
                collectFields,
                OperationCoreBuilder.BuildExecuteField(
                    coerceArgumentValues,
                    resolve,
                    completeValue
                )
            );

            return (context, _, _) =>
            {
                context.ExecuteSelectionSet = executeSelectionSet;
                return Task.CompletedTask;
            };
        }

        private static OperationExecutorSelector UseOperationExecutorSelector(
            ResolveFieldEventStream subscribe,
            CoerceValue coerceValue)
        {
            var collectFields = OperationCoreBuilder.BuildCollectFields(coerceValue);
            var coerceArgumentValues = OperationCoreBuilder.BuildCoerceArgumentValues(coerceValue);
            var executeQuery = OperationCoreBuilder.BuildExecuteOperation();
            var executeSubscription = OperationCoreBuilder.BuildExecuteSubscription(
                OperationCoreBuilder.BuildCreateSourceEventStream(
                    collectFields,
                    coerceArgumentValues,
                    subscribe
                ),
                OperationCoreBuilder.BuildMapSourceToResponseEvent(
                    OperationCoreBuilder.BuildExecuteSubscriptionEvent()
                )
            );

            return (context, _, _) =>
            {
                context.OperationExecutor = context.Operation
                    ?.Operation is OperationType.Query or OperationType.Mutation
                    ? executeQuery
                    : executeSubscription;

                return Task.CompletedTask;
            };
        }

        private static OperationSelector UseOperationSelector()
        {
            return (context, opts, _) =>
            {
                context.Operation = Ast.GetOperation(opts.Document, opts.OperationName);
                return Task.CompletedTask;
            };
        }
    }
}