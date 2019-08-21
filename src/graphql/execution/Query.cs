using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tanka.graphql.execution
{
    public static class Query
    {
        public static async Task<ExecutionResult> ExecuteQueryAsync(
            QueryContext context)
        {
            var (schema, _, operation, initialValue, coercedVariableValues) = context;
            var queryType = schema.Query;
            var path = new NodePath();

            if (queryType == null)
                throw new QueryExecutionException(
                    "Schema does not support queries. Query type is null.",
                    path);

            var selectionSet = operation.SelectionSet;
            var executionContext = context.BuildExecutorContext(new ParallelExecutionStrategy());

            IDictionary<string, object> data;

            try
            {
                data = await SelectionSets.ExecuteSelectionSetAsync(
                    executionContext,
                    selectionSet,
                    queryType,
                    initialValue,
                    path).ConfigureAwait(false);
            }
            catch (QueryExecutionException e)
            {
                executionContext.AddError(e);
                data = null;
            }

            return new ExecutionResult
            {
                Errors = executionContext
                    .FieldErrors
                    .Select(context.FormatError)
                    .ToList(),
                Data = data
            };
        }
    }
}