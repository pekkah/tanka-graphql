using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.error;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Mutation
    {
        public static async Task<ExecutionResult> ExecuteMutationAsync(
            IErrorTransformer errorTransformer,
            GraphQLDocument document,
            GraphQLOperationDefinition mutation,
            ExecutableSchema schema,
            Dictionary<string, object> coercedVariableValues,
            object initialValue)
        {
            var executionContext = new SerialExecutionContext(schema, document);

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
                Errors = executionContext.FieldErrors.SelectMany(errorTransformer.Transfrom).ToList(),
                Data = data
            };
        }
    }
}