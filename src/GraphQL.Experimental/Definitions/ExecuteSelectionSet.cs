using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task<SelectionSetResult> ExecuteSelectionSet(
        OperationContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        SelectionSet selectionSet,
        NodePath path,
        CancellationToken cancellationToken
    );
}