using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public delegate Task<object?> CompleteValue(
        OperationContext context,
        TypeBase fieldType,
        IReadOnlyList<FieldSelection> fields,
        object? resolvedValue,
        ResolveAbstractType resolveAbstractType,
        NodePath path,
        CancellationToken cancellationToken
    );
}