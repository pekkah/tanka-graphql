using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Query
    {
        public static async Task<ExecutionResult> ExecuteQueryAsync(
            IErrorTransformer errorTransformer,
            GraphQLDocument document,
            GraphQLOperationDefinition query,
            ISchema schema,
            Dictionary<string, object> coercedVariableValues,
            object initialValue)
        {
            var queryType = schema.Query;

            if (queryType == null)
                throw new GraphQLError(
                    $"Schema does not support queries. Query type is null.");

            var selectionSet = query.SelectionSet;
            var executionContext = new ExecutorContext(
                schema, 
                document,
                new ParallelExecutionStrategy());

            IDictionary<string, object> data = null;

            try
            {
                data = await SelectionSets.ExecuteSelectionSetAsync(
                    executionContext,
                    selectionSet,
                    queryType,
                    initialValue,
                    coercedVariableValues).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                executionContext.FieldErrors.Add(e);
                data = null;
            }

            return new ExecutionResult
            {
                Errors = executionContext.FieldErrors.SelectMany(errorTransformer.Transfrom).ToList(),
                Data = data
            };
        }
    }
}