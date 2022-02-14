using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class EnumValuesDefinition : CollectionNodeBase<EnumValueDefinition>
{
    public EnumValuesDefinition(IReadOnlyList<EnumValueDefinition> items, in Location? location = default) : base(items,
        in location)
    {
    }

    public override NodeKind Kind => NodeKind.EnumValuesDefinition;
}