using System.Threading;
using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public delegate ValueTask<ISubscribeResult> Subscriber(ResolverContext context, CancellationToken unsubscribe);
}