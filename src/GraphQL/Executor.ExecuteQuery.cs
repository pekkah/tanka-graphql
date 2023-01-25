using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL;

public partial class Executor
{
    public async Task<ExecutionResult> ExecuteQuery(QueryContext context)
    {
        var path = new NodePath();
        var rootType = context.OperationDefinition.Operation switch
        {
            OperationType.Query => context.Schema.Query,
            OperationType.Mutation => context.Schema.Mutation,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (rootType == null)
            throw new QueryException($"Schema does not support '{context.OperationDefinition.Operation}'. Root type not set.")
            {
                Path = path
            };

        var selectionSet = context.OperationDefinition.SelectionSet;

        try
        {
            var result = await context.ExecuteSelectionSet(
                selectionSet,
                rootType,
                context.Request.InitialValue,
                path);

            return new()
            {
                Data = result,
                Errors = context.ErrorCollector.GetErrors().ToList()
            };
        }
        catch (FieldException e)
        {
            context.ErrorCollector.Add(e);
        }

        return new()
        {
            Data = null,
            Errors = context.ErrorCollector.GetErrors().ToList()
        };
    }
}