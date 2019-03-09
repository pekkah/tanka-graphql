using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public delegate ValueTask<IResolveResult> ResolverMiddleware(ResolverContext context, Resolver next);
}