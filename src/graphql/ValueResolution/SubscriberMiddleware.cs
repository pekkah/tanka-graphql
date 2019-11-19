using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<ISubscribeResult> SubscriberMiddleware(IResolverContext context, CancellationToken unsubscribe, Subscriber next);
}