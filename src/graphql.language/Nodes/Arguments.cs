using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Arguments : CollectionNodeBase<Argument>
    {
        public Arguments(IReadOnlyList<Argument> items, in Location? location = default) : base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.Arguments;

        public static Arguments None = new Arguments(Array.Empty<Argument>());
    }
}