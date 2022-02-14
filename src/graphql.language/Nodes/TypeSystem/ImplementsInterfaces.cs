using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class ImplementsInterfaces : CollectionNodeBase<NamedType>
{
    public ImplementsInterfaces(IReadOnlyList<NamedType> items, in Location? location = default) : base(items,
        in location)
    {
    }

    public override NodeKind Kind => NodeKind.ImplementsInterfaces;

    public static ImplementsInterfaces? From(IReadOnlyList<NamedType>? interfaces)
    {
        if (interfaces == null)
            return null;

        return new ImplementsInterfaces(interfaces);
    }

    public bool TryGet(Name interfaceName, [NotNullWhen(true)] out NamedType? namedType)
    {
        foreach (var @interface in this)
        {
            if (@interface.Name != interfaceName) continue;

            namedType = @interface;
            return true;
        }

        namedType = null;
        return false;
    }
}