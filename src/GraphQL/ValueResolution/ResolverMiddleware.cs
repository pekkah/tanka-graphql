namespace Tanka.GraphQL.ValueResolution;

public delegate ValueTask ResolverMiddleware(ResolverContext context, Resolver next);