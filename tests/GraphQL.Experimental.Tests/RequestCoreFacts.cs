using System;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Xunit;
using static Tanka.GraphQL.Experimental.OperationCoreBuilder;
using static Tanka.GraphQL.Experimental.RequestCoreBuilder;

namespace Tanka.GraphQL.Experimental.Tests
{
    public class RequestCoreFacts
    {
        [Fact]
        public async Task Execute()
        {
            /* Given */
            object? initialValue = "initial";

            var collectFields = BuildCollectFields(Coerce.CoerceValue);
            var coerceArgumentValues = BuildCoerceArgumentValues(Coerce.CoerceValue);

            var executeSelectionSet = BuildExecuteSelectionSet(
                collectFields,
                BuildExecuteField(
                    coerceArgumentValues,
                    (context, objectDefinition, objectValue, fieldName, agumentValues, path, cancellationToken) =>
                        default,
                    (context, fieldType, fields, resolvedValue, path, cancellationToken) => default
                )
            );

            var executeQuery = BuildExecuteOperation(
                executeSelectionSet
            );

            var executeSubscription = BuildExecuteSubscription(
                BuildCreateSourceEventStream(
                    collectFields,
                    coerceArgumentValues,
                    resolveFieldEventStream
                    ),
                BuildMapSourceToResponseEvent(
                    BuildExecuteSubscriptionEvent(
                        executeSelectionSet
                        )
                    )
                );

            var executeRequest = BuildExecute(
                BuildCreateOperationContext(
                    (context, options, _) =>
                    {
                        context.Operation = Ast.GetOperation(options.Document, options.OperationName);
                        return Task.CompletedTask;
                    },
                    BuildCoerceVariableValues(Coerce.CoerceValue),
                    (context, options, _) => Task.CompletedTask,
                    (context, options, cancellationToken) =>
                    {
                        if (context.Operation?.Operation is OperationType.Query or OperationType.Mutation)
                            context.OperationExecutor = executeQuery;
                        else
                            context.OperationExecutor = executeSubscription;

                        return Task.CompletedTask;
                    },
                    Coerce.CoerceValue
                ));

            var executeRequestSingle = BuildExecuteSingle(executeRequest);

            /* When */
            var result = await executeRequestSingle(new RequestOptions(), initialValue, CancellationToken.None);

            /* Then */
        }
    }
}