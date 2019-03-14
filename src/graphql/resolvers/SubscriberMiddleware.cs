using System.Threading;
using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public delegate ValueTask<ISubscribeResult> SubscriberMiddleware(ResolverContext context, CancellationToken unsubscribe, Subscriber next);
}