using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class SelectionSet
    {
        public readonly Location? Location;
        public readonly IReadOnlyCollection<ISelection> Selections;

        public SelectionSet(
            IReadOnlyCollection<ISelection> selections,
            in Location? location = default)
        {
            Selections = selections;
            Location = location;
        }
    }
}