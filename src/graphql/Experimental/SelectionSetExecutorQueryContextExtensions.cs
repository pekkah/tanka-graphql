using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental;

public static class SelectionSetExecutorQueryContextExtensions
{
    public static Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        this QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        return context.SelectionSetExecutor.ExecuteSelectionSet(
            context,
            selectionSet,
            objectType,
            objectValue,
            path);
    }
}