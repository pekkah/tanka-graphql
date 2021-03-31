using System.Collections.Generic;
using System.Threading;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate IAsyncEnumerable<OperationResult> MapSourceToResponseEvent(
        OperationContext context,
        IAsyncEnumerable<object?> sourceStream,
        CancellationToken cancellationToken
    );
}