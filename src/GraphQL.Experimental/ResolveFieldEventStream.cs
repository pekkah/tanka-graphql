using System.Collections.Generic;
using System.Threading;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public delegate IAsyncEnumerable<object?> ResolveFieldEventStream(
        OperationContext context,
        ObjectDefinition subscriptionDefinition,
        object? rootValue,
        string fieldName,
        IReadOnlyDictionary<string, object?> argumentValues,
        CancellationToken cancellationToken
    );
}