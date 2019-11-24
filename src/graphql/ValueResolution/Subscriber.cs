using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<ISubscriberResult> Subscriber(IResolverContext context, CancellationToken unsubscribe);
}