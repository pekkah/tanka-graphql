using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;
using static Tanka.GraphQL.Experimental.Core.OperationCoreBuilder;
using static Tanka.GraphQL.Experimental.Core.RequestCoreBuilder;

namespace Tanka.GraphQL.Experimental.Tests.Core
{
    public class RequestCoreFacts
    {
        public RequestCoreFacts()
        {
            Schema = @"
type World {
    hello: String!
}

type Query {
    hello: String!
    world: World
}
";

            Data = new ResolverRoutes
            {
                {"Query.hello", ResolveHello},
                {"Query.world", ResolveWorld},
                {"World.hello", ResolveHello}
            };
        }

        private ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)> ResolveWorld(
            OperationContext context, 
            ObjectDefinition objectdefinition, 
            object? objectvalue, 
            Name fieldname, 
            IReadOnlyDictionary<string, object?> coercedargumentvalues, 
            NodePath path,
            CancellationToken cancellationtoken)
        {
            (object?, ResolveAbstractType?) value = ("Hello", null);
            return new ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)>(value);
        }

        public ResolverRoutes Data { get; }

        public ExecutableSchema Schema { get; }

        private ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)> ResolveHello(
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

            ResolveFieldValue resolveFieldValue =
                (context, objectDefinition, objectValue, fieldName, variableValues, path, token) =>
                    Data.Resolver($"{objectDefinition.Name}.{fieldName}")(context, objectDefinition, objectValue,
                        fieldName, variableValues, path, token);

            var completeValue = BuildCompleteValue(
                serializeValue
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
                        world {
                            hello
                        }
                    }",
                Schema = Schema
            }, initialValue, CancellationToken.None);

            /* Then */
            result.ShouldMatchJson(@"{
                  ""data"": {
                    ""hello"": ""World"",
                    ""world"": {
                        ""hello"": ""World""
                    }
                  }
                }");
        }
    }
}