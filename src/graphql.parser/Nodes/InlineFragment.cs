using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class InlineFragment: ISelection
    {
        public SelectionType SelectionType => SelectionType.InlineFragment;

        public readonly NamedType? TypeCondition;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly SelectionSet SelectionSet;
        public readonly Location? Location;

        public InlineFragment(
            in NamedType? typeCondition,
            in IReadOnlyCollection<Directive>? directives,
            in SelectionSet selectionSet,
            in Location? location)
        {
            TypeCondition = typeCondition;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }

    }
}