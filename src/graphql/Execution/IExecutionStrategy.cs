using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Execution;

public interface IExecutionStrategy
{
    Task<IDictionary<string, object?>> ExecuteGroupedFieldSetAsync(
        IExecutorContext context,
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
        ObjectDefinition objectDefinition,
        object? objectValue,
        NodePath path);
}