using System.Collections.Generic;
using System.Threading;

namespace Tanka.GraphQL.Experimental
{
    public delegate IAsyncEnumerable<OperationResult> OperationExecutor(
        OperationContext context,
        RequestOptions options,
        CancellationToken cancellationToken
    );
}