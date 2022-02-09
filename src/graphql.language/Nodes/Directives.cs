using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        public bool TryGet(Name directiveName, [NotNullWhen(true)] out Directive? directive)
        {
            foreach (var directiveThis in this)
            {
                if (directiveThis.Name != directiveName) continue;
                
                directive = directiveThis;
                return true;
            }

            directive = null;
            return false;
        }
    }
}