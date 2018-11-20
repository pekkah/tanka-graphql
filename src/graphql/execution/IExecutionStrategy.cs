using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public interface IExecutionStrategy
    {
        Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(IExecutorContext context,
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            Dictionary<string, object> coercedVariableValues, NodePath path);
    }
}