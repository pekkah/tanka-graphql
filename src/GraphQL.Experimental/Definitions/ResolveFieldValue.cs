using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)> ResolveFieldValue(
        OperationContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        Name fieldName,
        IReadOnlyDictionary<string, object?> coercedArgumentValues,
        NodePath path,
        CancellationToken cancellationToken
    );
}