using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task<OperationResult> ExecuteRequestSingle(
        RequestOptions options,
        object? initialValue = null,
        CancellationToken cancellationToken = default
    );
}