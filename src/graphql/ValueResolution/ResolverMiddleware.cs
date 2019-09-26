using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<IResolveResult> ResolverMiddleware(ResolverContext context, Resolver next);
}