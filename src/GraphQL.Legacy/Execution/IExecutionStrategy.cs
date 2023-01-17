using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Execution;

public interface IExecutionStrategy
{
    Task<IDictionary<string, object?>?> ExecuteSelectionSetAsync(
        IExecutorContext executorContext,
        SelectionSet selectionSet,
        ObjectDefinition objectDefinition,
        object objectValue,
        NodePath path);
}