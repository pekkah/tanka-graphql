using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public delegate ValueTask<object?> Resolver(ResolverContext context);