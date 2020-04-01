using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public interface IExecutionStrategy
    {
        Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            IExecutorContext context,
            IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
            ObjectType objectType, 
            object objectValue, 
            NodePath path);
    }
}