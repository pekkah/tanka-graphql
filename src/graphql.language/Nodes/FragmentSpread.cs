using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FragmentSpread : ISelection
    {
        public readonly IReadOnlyCollection<Directive>? Directives;

        public readonly Name FragmentName;
        public readonly Location? Location;

        public FragmentSpread(
            Name fragmentName,
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