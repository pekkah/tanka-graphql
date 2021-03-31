using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task OperationSelector(
        OperationPlanContext context,
        RequestOptions options,
        CancellationToken cancellationToken
    );
}