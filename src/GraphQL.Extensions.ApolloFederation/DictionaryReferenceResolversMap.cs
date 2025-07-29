using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public class DictionaryReferenceResolversMap : Dictionary<string, ResolveReference>, IReferenceResolversMap
{
    public bool TryGetReferenceResolver(string type, [NotNullWhen(true)] out ResolveReference? resolveReference)
    {
        return TryGetValue(type, out resolveReference);
    }
}