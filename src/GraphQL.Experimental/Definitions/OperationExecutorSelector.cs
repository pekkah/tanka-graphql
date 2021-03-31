using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task OperationExecutorSelector(
        OperationPlanContext context,
        RequestOptions options,
        CancellationToken cancellationToken
    );
}