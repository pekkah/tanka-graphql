using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<ISubscribeResult> Subscriber(IResolverContext context, CancellationToken unsubscribe);
}