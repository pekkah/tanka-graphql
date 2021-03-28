using System.Collections.Generic;
using System.Threading;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public delegate Dictionary<string, List<FieldSelection>> CollectFields(
        OperationContext context,
        ObjectDefinition objectType,
        SelectionSet selectionSet,
        List<string>? visitedFragments = null,
        CancellationToken cancellationToken = default
    );
}