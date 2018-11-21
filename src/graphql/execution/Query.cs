using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.error;

namespace fugu.graphql.execution
{
    public static class Query
    {
        public static async Task<ExecutionResult> ExecuteQueryAsync(
            QueryContext context)
        {
            var (schema, _, operation, initialValue, coercedVariableValues) = context;
            var queryType = schema.Query;

            if (queryType == null)
                throw new GraphQLError(
                    "Schema does not support queries. Query type is null.");

            var selectionSet = operation.SelectionSet;
            var executionContext = context.BuildExecutorContext(new ParallelExecutionStrategy());

            IDictionary<string, object> data = null;

            try
            {
                var path = new NodePath();
                data = await SelectionSets.ExecuteSelectionSetAsync(
                    executionContext,
                    selectionSet,
                    queryType,
                    initialValue,
                    coercedVariableValues,
                    path).ConfigureAwait(false);
            }
            catch (GraphQLError e)
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