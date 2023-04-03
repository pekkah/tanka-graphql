using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class FragmentDefinitions : CollectionNodeBase<FragmentDefinition>
{
    public static FragmentDefinitions None = new(Array.Empty<FragmentDefinition>());

    public FragmentDefinitions(IReadOnlyList<FragmentDefinition> items, in Location? location = default) : base(items,
        in location)
    {
    }

    public override NodeKind Kind => NodeKind.FragmentDefinitions;
}