using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public delegate ValueTask<IReadOnlyDictionary<string, object?>> CoerceArgumentValues(
        ExecutableSchema schema,
        ObjectDefinition objectDefinition,
        FieldSelection field,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        CancellationToken cancellationToken
    );
}