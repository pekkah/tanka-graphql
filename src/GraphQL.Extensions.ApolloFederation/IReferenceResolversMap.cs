using System.Diagnostics.CodeAnalysis;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public interface IReferenceResolversMap
{
    bool TryGetReferenceResolver(string type, [NotNullWhen(true)]out ResolveReference? resolveReference);
}