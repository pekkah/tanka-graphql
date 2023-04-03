using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class VariableDefinitions : CollectionNodeBase<VariableDefinition>
{
    public VariableDefinitions(IReadOnlyList<VariableDefinition> items, in Location? location = default) : base(items,
        in location)
    {
    }

    public override NodeKind Kind => NodeKind.VariableDefinitions;
}