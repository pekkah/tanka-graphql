using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate Task ValidateOperation(
        OperationPlanContext context,
        RequestOptions options,
        CancellationToken cancellationToken
    );
}