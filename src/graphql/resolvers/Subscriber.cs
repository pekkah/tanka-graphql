using System.Threading;
using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public delegate Task<ISubscribeResult> Subscriber(ResolverContext context, CancellationToken cancellationToken = default(CancellationToken));
}