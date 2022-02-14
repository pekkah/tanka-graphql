namespace Tanka.GraphQL.Extensions.ApolloFederation;

public interface IReferenceResolversMap
{
    bool TryGetReferenceResolver(string type, out ResolveReference resolveReference);
}