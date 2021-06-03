using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Core2
{
    public class SelectionSetExecutor
    {
        public Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            SelectionSet selectionSet,
            NodePath path,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var strategy = context.ExecuteSelectionSet;
            return strategy(context, objectDefinition, objectValue, selectionSet, path, cancellationToken);
        }
    }
}