using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FragmentDefinition
    {
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly Name FragmentName;
        public readonly Location? Location;
        public readonly SelectionSet SelectionSet;
        public readonly NamedType TypeCondition;

        public FragmentDefinition(
            Name fragmentName,
            NamedType typeCondition,
            IReadOnlyCollection<Directive>? directives,
            SelectionSet selectionSet,
            in Location? location)
        {
            FragmentName = fragmentName;
            TypeCondition = typeCondition;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }
    }
}