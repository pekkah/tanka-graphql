using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public delegate ValueTask<ISubscribeResult> SubscriberMiddleware(ResolverContext context, CancellationToken unsubscribe, Subscriber next);
}