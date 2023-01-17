using Tanka.GraphQL.Fields;

namespace Tanka.GraphQL;

public delegate ValueTask ResolverMiddleware(ResolverContext context, Resolver next);