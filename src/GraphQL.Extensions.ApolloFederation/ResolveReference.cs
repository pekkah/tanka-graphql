using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public delegate ValueTask<ResolveReferenceResult> ResolveReference(
        IResolverContext context,
        INamedType type,
        IReadOnlyDictionary<string, object> representation);

    public readonly struct ResolveReferenceResult
    {
        public INamedType Type { get; }

        public object Reference { get; }

        public ResolveReferenceResult(INamedType type, object reference)
        {
            Type = type;
            Reference = reference;
        }

        public void Deconstruct(out INamedType type, out object reference)
        {
            type = Type;
            reference = Reference;
        }
    }
}