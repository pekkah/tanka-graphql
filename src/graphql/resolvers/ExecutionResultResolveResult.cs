using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.resolvers
{
    public class ExecutionResultResolveResult : IResolveResult
    {
        private readonly ExecutionResult _result;

        public ExecutionResultResolveResult(ExecutionResult result)
        {
            Value = result;
            _result = result;
        }

        public object Value { get; }

        public Task<object> CompleteValueAsync(
            IExecutorContext executorContext, 
            ObjectType objectType, 
            IField field, 
            IType fieldType,
            GraphQLFieldSelection selection, 
            List<GraphQLFieldSelection> fields, 
            Dictionary<string, object> coercedVariableValues, 
            NodePath path)
        {
            if (_result.Data.ContainsKey(selection.Name.Value))
            {
                return Task.FromResult(_result.Data[selection.Name.Value]);
            }

            return Task.FromResult(default(object));
        }
    }
}