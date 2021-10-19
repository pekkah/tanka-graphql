using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Directives : CollectionNodeBase<Directive>
    {
        public static Directives None = new Directives(Array.Empty<Directive>());

        public Directives(
            IReadOnlyList<Directive> items,
            in Location? location = default)
            : base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.Directives;

        public static Directives? From(
            IReadOnlyList<Directive>? directives)
        {
            if (directives == null)
                return null;

            return new Directives(directives);
        }
    }
}