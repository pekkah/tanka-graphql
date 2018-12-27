using System.Threading.Tasks;

namespace fugu.graphql.resolvers
{
    public delegate Task<IResolveResult> Resolver(ResolverContext context);
}