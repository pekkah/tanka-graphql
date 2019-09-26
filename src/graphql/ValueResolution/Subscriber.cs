using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<ISubscribeResult> Subscriber(ResolverContext context, CancellationToken unsubscribe);
}