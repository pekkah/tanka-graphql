using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class OperationDefinitions : CollectionNodeBase<OperationDefinition>
{
    public OperationDefinitions(IReadOnlyList<OperationDefinition> items, in Location? location = default) : base(items,
        in location)
    {
    }

    public override NodeKind Kind => NodeKind.OperationDefinitions;
}