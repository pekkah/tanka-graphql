using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class UnionMemberTypes : CollectionNodeBase<NamedType>
{
    public UnionMemberTypes(IReadOnlyList<NamedType> items, in Location? location = default) : base(items, in location)
    {
    }

    public override NodeKind Kind => NodeKind.UnionMemberTypes;

    public static UnionMemberTypes? From(IReadOnlyList<NamedType>? members)
    {
        if (members == null)
            return null;

        return new UnionMemberTypes(members);
    }
}