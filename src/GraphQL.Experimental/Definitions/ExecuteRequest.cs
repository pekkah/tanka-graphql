using System.Collections.Generic;
using System.Threading;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate IAsyncEnumerable<OperationResult> ExecuteRequest(
        RequestOptions options,
        object? initialValue = null,
        CancellationToken cancellationToken = default
    );
}