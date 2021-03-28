using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental
{
    public delegate Task<OperationContext> CreateOperationContext(
        RequestOptions options,
        object? initialValue = null,
        CancellationToken cancellationToken = default
    );
}