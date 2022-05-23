using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public interface IResolverMiddleware
{
    ValueTask<IResolverResult> Invoke(Resolver next, IResolverContext context);
}