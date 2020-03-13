using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FragmentDefinition
    {
        public readonly Name FragmentName;
        public readonly NamedType TypeCondition;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly SelectionSet SelectionSet;
        public readonly Location? Location;

        public FragmentDefinition(
            in Name fragmentName, 
            in NamedType typeCondition, 
            in IReadOnlyCollection<Directive>? directives, 
            in SelectionSet selectionSet,
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