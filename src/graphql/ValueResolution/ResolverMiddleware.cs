using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<IResolveResult> ResolverMiddleware(IResolverContext context, Resolver next);
}