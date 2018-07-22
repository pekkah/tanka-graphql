using System.Threading.Tasks;
using fugu.graphql.type;

namespace fugu.graphql.resolvers
{
    public delegate Task<IResolveResult> Resolver(ResolverContext context);
}