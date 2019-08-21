using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public interface IExecutionStrategy
    {
        Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            IExecutorContext context,
            IReadOnlyDictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, 
            object objectValue, 
            NodePath path);
    }
}