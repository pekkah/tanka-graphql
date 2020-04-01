using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FragmentSpread : ISelection, INode
    {
        public NodeKind Kind => NodeKind.FragmentSpread;
        public IReadOnlyCollection<Directive>? Directives {get;}

        public readonly Name FragmentName;
        public Location? Location {get;}

        public FragmentSpread(
            in Name fragmentName,
            IReadOnlyCollection<Directive>? directives,
            in Location? location = default)
        {
            FragmentName = fragmentName;
            Directives = directives;
            Location = location;
        }

        public SelectionType SelectionType => SelectionType.FragmentSpread;
    }
}