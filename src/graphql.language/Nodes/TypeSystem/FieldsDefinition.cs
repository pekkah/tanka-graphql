using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class FieldsDefinition : CollectionNodeBase<FieldDefinition>
{
    public FieldsDefinition(IReadOnlyList<FieldDefinition> items, in Location? location = default) : base(items,
        in location)
    {
    }

    public override NodeKind Kind => NodeKind.FieldsDefinition;

    public static FieldsDefinition? From(IReadOnlyList<FieldDefinition>? fields)
    {
        if (fields == null)
            return null;

        return new FieldsDefinition(fields);
    }
}