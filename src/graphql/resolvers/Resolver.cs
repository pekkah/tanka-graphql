using System.Threading.Tasks;

namespace tanka.graphql.resolvers
{
    public delegate ValueTask<IResolveResult> Resolver(ResolverContext context);
}