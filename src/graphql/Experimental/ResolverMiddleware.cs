using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public delegate ValueTask ResolverMiddleware(ResolverContext context, Resolver next);