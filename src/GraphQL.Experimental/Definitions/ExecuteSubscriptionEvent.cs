using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task<OperationResult> ExecuteSubscriptionEvent(
        OperationContext context,
        object? @event,
        CancellationToken cancellationToken
    );
}