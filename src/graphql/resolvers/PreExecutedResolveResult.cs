using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.resolvers
{
    public class PreExecutedResolveResult : IResolveResult
    {
        private readonly IDictionary<string, object> _data;

        public PreExecutedResolveResult(IDictionary<string, object> data)
        {
            _data = data;
        }

        public object Value => _data;

        public Task<object> CompleteValueAsync(IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IType fieldType,
            GraphQLFieldSelection selection,
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            NodePath path)
        {
            var value = _data[selection.Name.Value];
            var resolveResult = new ResolveResult(value);
            return resolveResult.CompleteValueAsync(
                executorContext,
                objectType,
                field,
                fieldType,
                selection,
                fields,
                path);
        }
    }
}