using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public delegate ValueTask<(object Reference, INamedType? NamedType)> ResolveReference(
        IResolverContext context,
        IReadOnlyDictionary<string, object> representation,
        INamedType type);
}