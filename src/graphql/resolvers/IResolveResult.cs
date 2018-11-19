using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.execution;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.resolvers
{
    public interface IResolveResult
    {
        object Value { get; }

        ObjectType ActualType { get; }

        Task<object> CompleteValueAsync(
            IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IGraphQLType fieldType,
            GraphQLFieldSelection selection,
            List<GraphQLFieldSelection> fields,
            Dictionary<string, object> coercedVariableValues);
    }
}