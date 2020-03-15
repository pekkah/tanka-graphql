using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FieldSelection : ISelection
    {
        public readonly Location Location;
        public readonly Name? Alias;
        public readonly Name Name;
        public readonly IReadOnlyCollection<Argument>? Arguments;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly SelectionSet? SelectionSet;

        public FieldSelection(
            in Name? alias,
            in Name name,
            in IReadOnlyCollection<Argument>? arguments,
            in IReadOnlyCollection<Directive>? directives,
            in SelectionSet? selectionSet,
            in Location location)
        {
            Alias = alias;
            Name = name;
            Arguments = arguments;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }

        public SelectionType SelectionType => SelectionType.Field;
    }
}