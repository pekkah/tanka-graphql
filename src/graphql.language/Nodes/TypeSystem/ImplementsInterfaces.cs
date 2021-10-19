using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ImplementsInterfaces : CollectionNodeBase<NamedType>
    {
        public ImplementsInterfaces(IReadOnlyList<NamedType> items, in Location? location = default) : base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.ImplementsInterfaces;

        public static ImplementsInterfaces? From(IReadOnlyList<NamedType>? interfaces)
        {
            if (interfaces == null)
                return null;

            return new ImplementsInterfaces(interfaces);
        }
    }
}