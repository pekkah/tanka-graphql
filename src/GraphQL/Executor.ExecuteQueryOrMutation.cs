using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL;

public partial class Executor
{
    public static async Task ExecuteQueryOrMutation(QueryContext context)
    {
        var path = new NodePath();
        ObjectDefinition? rootType = context.OperationDefinition.Operation switch
        {
            OperationType.Query => context.Schema.Query,
            OperationType.Mutation => context.Schema.Mutation,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (rootType == null)
            throw new QueryException(
                $"Schema does not support '{context.OperationDefinition.Operation}'. Root type not set.")
            {
                Path = path
            };

        SelectionSet selectionSet = context.OperationDefinition.SelectionSet;

        try
        {
            IReadOnlyDictionary<string, object?> result = await context.ExecuteSelectionSet(
                selectionSet,
                rootType,
                context.Request.InitialValue,
                path);

            context.Response = AsyncEnumerableEx.Return(new ExecutionResult
            {
                Data = result,
                Errors = context.GetErrors().ToList()
            });
            return;
        }
        catch (FieldException e)
        {
            context.AddError(e);
        }

        context.Response = AsyncEnumerableEx.Return(new ExecutionResult
        {
            Data = null,
            Errors = context.GetErrors().ToList()
        });
    }
}