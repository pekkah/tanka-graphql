using System.Collections.Generic;
using System.Threading;

namespace Tanka.GraphQL.Experimental
{
    public delegate IAsyncEnumerable<OperationResult> MapSourceToResponseEvent(
        OperationContext context,
        IAsyncEnumerable<object?> sourceStream,
        CancellationToken cancellationToken
    );
}