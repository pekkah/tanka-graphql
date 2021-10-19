using System.Collections.Generic;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public class DictionaryReferenceResolversMap : Dictionary<string, ResolveReference>, IReferenceResolversMap
    {
        public bool TryGetReferenceResolver(string type, out ResolveReference resolveReference)
        {
            return TryGetValue(type, out resolveReference);
        }
    }
}