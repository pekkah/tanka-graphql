using System.Linq;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Execution;

public static class Mutation
{
    public static async Task<ExecutionResult> ExecuteMutationAsync(
        QueryContext context)
    {
        var (schema, _, operation, initialValue, coercedVariableValues) = context;
        var executionContext = context.BuildExecutorContext(new SerialExecutionStrategy());
        var path = new NodePath();

        var mutationType = schema.Mutation;
        if (mutationType == null)
            throw new QueryExecutionException(
                "Schema does not support mutations. Mutation type is null.",
                path);

        var selectionSet = operation.SelectionSet;
        var data = await executionContext.Strategy.ExecuteSelectionSetAsync(
            executionContext,
            selectionSet,
            mutationType,
            initialValue,
            path).ConfigureAwait(false);


        return new ExecutionResult
        {
            Errors = executionContext
                .FieldErrors
                .Select(context.FormatError).ToList(),
            Data = data?.ToDictionary(kv => kv.Key, kv => kv.Value)
        };
    }
}