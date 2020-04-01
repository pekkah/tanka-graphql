using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FieldSelection : ISelection, INode
    {
        public NodeKind Kind => NodeKind.FieldSelection;
        public readonly Name? Alias;
        public readonly IReadOnlyCollection<Argument>? Arguments;
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location {get;}
        public readonly Name Name;
        public readonly SelectionSet? SelectionSet;

        public Name AliasOrName => Alias ?? Name;

        public FieldSelection(
            in Name? alias,
            in Name name,
            IReadOnlyCollection<Argument>? arguments,
            IReadOnlyCollection<Directive>? directives,
            SelectionSet? selectionSet,
            in Location? location = default)
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