using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes;
using static Tanka.GraphQL.Experimental.Core.OperationCoreBuilder;
using static Tanka.GraphQL.Experimental.Core.RequestCoreBuilder;

namespace Tanka.GraphQL.Experimental
{
    public static class Request
    {
        public static ExecuteRequestSingle UseExecuteRequestSingle(
            ExecutableSchema schema,
            ValidateOperation validateOperation,
            ResolveFieldValue resolve,
            ResolveFieldEventStream subscribe,
            CoerceValue coerceValue,
            SerializeValue serializeValue)
        {
            var collectFields = BuildCollectFields(coerceValue);
            var coerceArgumentValues = BuildCoerceArgumentValues(coerceValue);

            var completeValue = BuildCompleteValue(
                serializeValue
            );

            var executeSelectionSet = BuildExecuteSelectionSet(
                collectFields,
                BuildExecuteField(
                    coerceArgumentValues,
                    resolve,
                    completeValue
                )
            );

            var executeQuery = BuildExecuteOperation();
            var executeSubscription = BuildExecuteSubscription(
                BuildCreateSourceEventStream(
                    collectFields,
                    coerceArgumentValues,
                    subscribe
                ),
                BuildMapSourceToResponseEvent(
                    BuildExecuteSubscriptionEvent()
                )
            );

            var executeRequest = BuildExecute(
                BuildCreateOperationContext(
                    (context, opts, _) =>
                    {
                        context.Operation = Ast.GetOperation(opts.Document, opts.OperationName);
                        return Task.CompletedTask;
                    },
                    BuildCoerceVariableValues(coerceValue),
                    validateOperation,
                    (context, _, _) =>
                    {
                        context.OperationExecutor = context.Operation
                            ?.Operation is OperationType.Query or OperationType.Mutation
                            ? executeQuery : executeSubscription;

                        return Task.CompletedTask;
                    },
                    (context, _, _) =>
                    {
                        context.ExecuteSelectionSet = executeSelectionSet;
                        return Task.CompletedTask;
                    },
                    coerceValue
                ));

            var executeRequestSingle = BuildExecuteSingle(executeRequest);

            return (requestOptions, initialValue, cancellationToken) =>
            {
                var executeOptions = requestOptions with
                {
                    Schema = schema
                };

                return executeRequestSingle(executeOptions, initialValue, cancellationToken);
            };
        }

        public static ExecuteRequest UseExecuteRequest(
            ExecutableSchema schema,
            ValidateOperation validateOperation,
            ResolveFieldValue resolve,
            ResolveFieldEventStream subscribe,
            CoerceValue coerceValue,
            SerializeValue serializeValue)
        {
            var collectFields = BuildCollectFields(coerceValue);
            var coerceArgumentValues = BuildCoerceArgumentValues(coerceValue);

            var completeValue = BuildCompleteValue(
                serializeValue
            );

            var executeSelectionSet = BuildExecuteSelectionSet(
                collectFields,
                BuildExecuteField(
                    coerceArgumentValues,
                    resolve,
                    completeValue
                )
            );

            var executeQuery = BuildExecuteOperation();
            var executeSubscription = BuildExecuteSubscription(
                BuildCreateSourceEventStream(
                    collectFields,
                    coerceArgumentValues,
                    subscribe
                ),
                BuildMapSourceToResponseEvent(
                    BuildExecuteSubscriptionEvent()
                )
            );

            var executeRequest = BuildExecute(
                BuildCreateOperationContext(
                    (context, opts, _) =>
                    {
                        context.Operation = Ast.GetOperation(opts.Document, opts.OperationName);
                        return Task.CompletedTask;
                    },
                    BuildCoerceVariableValues(coerceValue),
                    validateOperation,
                    (context, _, _) =>
                    {
                        context.OperationExecutor = context.Operation
                            ?.Operation is OperationType.Query or OperationType.Mutation 
                            ? executeQuery : executeSubscription;

                        return Task.CompletedTask;
                    },
                    (context, _, _) =>
                    {
                        context.ExecuteSelectionSet = executeSelectionSet;
                        return Task.CompletedTask;
                    },
                    coerceValue
                ));

            return (requestOptions, initialValue, cancellationToken) =>
            {
                var executeOptions = requestOptions with
                {
                    Schema = schema
                };

                return executeRequest(executeOptions, initialValue, cancellationToken);
            };
        }
    }
}
