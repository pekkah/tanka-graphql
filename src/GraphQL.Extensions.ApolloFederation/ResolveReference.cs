using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

public delegate ValueTask<ResolveReferenceResult> ResolveReference(
    IResolverContext context,
    TypeDefinition type,
    IReadOnlyDictionary<string, object> representation);

public readonly struct ResolveReferenceResult
{
    public TypeDefinition Type { get; }

    public object Reference { get; }

    public ResolveReferenceResult(TypeDefinition type, object reference)
    {
        Type = type;
        Reference = reference;
    }

    public void Deconstruct(out TypeDefinition type, out object reference)
    {
        type = Type;
        reference = Reference;
    }
}