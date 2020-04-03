using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class InlineFragment : ISelection, INode
    {
        public NodeKind Kind => NodeKind.InlineFragment;
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location {get;}
        public readonly SelectionSet SelectionSet;

        public readonly NamedType? TypeCondition;

        public InlineFragment(
            NamedType? typeCondition,
            IReadOnlyCollection<Directive>? directives,
            SelectionSet selectionSet,
            in Location? location = default)
        {
            TypeCondition = typeCondition;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }

        public SelectionType SelectionType => SelectionType.InlineFragment;
    }
}