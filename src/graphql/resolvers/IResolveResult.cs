using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.execution;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.resolvers
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
            Dictionary<string, object> coercedVariableValues, 
            NodePath path);
    }
}