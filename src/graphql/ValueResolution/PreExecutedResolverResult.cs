using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public class PreExecutedResolverResult : IResolverResult
    {
        private readonly IDictionary<string, object> _data;

        public PreExecutedResolverResult(IDictionary<string, object> data)
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
            var resolveResult = new ResolverResult(value);
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