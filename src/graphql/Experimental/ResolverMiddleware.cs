using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public delegate ValueTask<object?> ResolverMiddleware(ResolverContext context, Resolver next);