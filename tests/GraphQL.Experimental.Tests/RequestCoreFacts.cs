using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;
using static Tanka.GraphQL.Experimental.OperationCoreBuilder;
using static Tanka.GraphQL.Experimental.RequestCoreBuilder;

namespace Tanka.GraphQL.Experimental.Tests
{
    public class RequestCoreFacts
    {
        public RequestCoreFacts()
        {
            Schema = @"
type Query {
    hello: String!
}
";

            Data = new ResolverRoutes
            {
                {"Query.hello", Resolver}
            };
        }

        public ResolverRoutes Data { get; }

        public ExecutableSchema Schema { get; }

        private ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)> Resolver(
            OperationContext context,
            ObjectDefinition objectdefinition,
            object? objectvalue,
            Name fieldname,
            IReadOnlyDictionary<string, object?> coercedargumentvalues,
            NodePath path,
            CancellationToken cancellationtoken)
        {
            (object?, ResolveAbstractType?) value = ("World", null);
            return new ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)>(value);
        }

        [Fact]
        public async Task Execute()
        {
            /* Given */
            object? initialValue = "initial";


            var collectFields = BuildCollectFields(Coerce.CoerceValue);
            var coerceArgumentValues = BuildCoerceArgumentValues(Coerce.CoerceValue);
            SerializeValue serializeValue = (schema, definition, value) => new ValueTask<object?>(value);
            MergeSelectionSets mergeSelectionSets = fields => default;


            ResolveFieldValue resolveFieldValue =
                (context, objectDefinition, objectValue, fieldName, variableValues, path, token) =>
                    Data.Resolver($"{objectDefinition.Name}.{fieldName}")(context, objectDefinition, objectValue,
                        fieldName, variableValues, path, token);

            var completeValue = BuildCompleteValue(
                serializeValue,
                mergeSelectionSets
            );

            var executeSelectionSet = BuildExecuteSelectionSet(
                collectFields,
                BuildExecuteField(
                    coerceArgumentValues,
                    resolveFieldValue,
                    completeValue
                )
            );

            var executeQuery = BuildExecuteOperation();

            ResolveFieldEventStream resolveFieldEventStream = default;
            var executeSubscription = BuildExecuteSubscription(
                BuildCreateSourceEventStream(
                    collectFields,
                    coerceArgumentValues,
                    resolveFieldEventStream
                ),
                BuildMapSourceToResponseEvent(
                    BuildExecuteSubscriptionEvent()
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
                    (context, _, _) =>
                    {
                        context.ExecuteSelectionSet = executeSelectionSet;
                        return Task.CompletedTask;
                    },
                    Coerce.CoerceValue
                ));

            var executeRequestSingle = BuildExecuteSingle(executeRequest);

            /* When */
            var result = await executeRequestSingle(new RequestOptions
            {
                Document = @"{
                        hello
                    }",
                Schema = Schema
            }, initialValue, CancellationToken.None);

            /* Then */
            result.ShouldMatchJson(@"{
                  ""data"": {
                    ""hello"": ""World""
                    }
                }");
        }
    }
}