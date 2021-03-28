using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public delegate ValueTask<object?> CompleteValue(
        OperationContext context,
        TypeBase fieldType,
        IReadOnlyList<FieldSelection> fields,
        object? resolvedValue,
        NodePath path,
        CancellationToken cancellationToken
    );
}