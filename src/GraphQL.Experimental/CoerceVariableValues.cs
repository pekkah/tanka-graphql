using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental
{
    public delegate Task CoerceVariableValues(
        OperationPlanContext context,
        RequestOptions options,
        CancellationToken cancellationToken
    );
}