using System.Threading.Tasks;

namespace Tanka.GraphQL.ValueResolution;

public delegate ValueTask<IResolverResult> Resolver(IResolverContext context);