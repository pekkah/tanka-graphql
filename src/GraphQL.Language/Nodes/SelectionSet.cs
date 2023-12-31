using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class SelectionSet : CollectionNodeBase<ISelection>
{
    public SelectionSet(
        IReadOnlyList<ISelection> selections,
        in Location? location = default) : base(selections, in location)
    {
    }

    public override NodeKind Kind => NodeKind.SelectionSet;
}