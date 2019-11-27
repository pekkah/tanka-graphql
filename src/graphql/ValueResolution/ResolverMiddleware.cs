using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution
{
    public delegate ValueTask<IResolverResult> ResolverMiddleware(IResolverContext context, Resolver next);
}