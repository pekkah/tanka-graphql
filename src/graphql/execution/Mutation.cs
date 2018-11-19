using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Mutation
    {
        public static async Task<ExecutionResult> ExecuteMutationAsync(
            Func<GraphQLError, Error> formatError,
            GraphQLDocument document,
            GraphQLOperationDefinition mutation,
            ISchema schema,
            Dictionary<string, object> coercedVariableValues,
            object initialValue)
        {
            var executionContext = new ExecutorContext(
                schema, 
                document,
                new SerialExecutionStrategy());

            var mutationType = schema.Mutation;
            if (mutationType == null)
                throw new GraphQLError(
                    $"Schema does not support mutations. Mutation type is null.");

            var selectionSet = mutation.SelectionSet;
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                executionContext,
                selectionSet,
                mutationType,
                initialValue,
                coercedVariableValues).ConfigureAwait(false);


            return new ExecutionResult
            {
                Errors = executionContext.FieldErrors.Select(formatError).ToList(),
                Data = data
            };
        }
    }
}