using System.Collections.Generic;
using System.Threading;

namespace Tanka.GraphQL.Experimental
{
    public delegate IAsyncEnumerable<object?> CreateSourceEventStream(
        OperationContext context,
        object? initialValue,
        CancellationToken cancellationToken);
}