using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public delegate ValueTask<IResolveResult> ResolverMiddleware(ResolverContext context, Resolver next);
}