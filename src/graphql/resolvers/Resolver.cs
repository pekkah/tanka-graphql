using System.Threading.Tasks;
using tanka.graphql.type;

namespace tanka.graphql.resolvers
{
    public delegate Task<IResolveResult> Resolver(ResolverContext context);
}