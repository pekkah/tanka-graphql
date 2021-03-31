using System.Collections.Generic;
using System.Threading;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate IAsyncEnumerable<OperationResult> OperationExecutor(
        OperationContext context,
        RequestOptions options,
        object? objectValue,
        CancellationToken cancellationToken
    );
}