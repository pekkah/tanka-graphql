using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FragmentSpread : ISelection
    {
        public SelectionType SelectionType => SelectionType.FragmentSpread;

        public readonly Name FragmentName;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly Location? Location;

        public FragmentSpread(
            in Name fragmentName,
            in IReadOnlyCollection<Directive>? directives,
            in Location? location)
        {
            FragmentName = fragmentName;
            Directives = directives;
            Location = location;
        }
    }
}