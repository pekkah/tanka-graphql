using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class SelectionSet
    {
        public readonly Location Location;
        public readonly IReadOnlyCollection<ISelection> Selections;

        public SelectionSet(
            in IReadOnlyCollection<ISelection> selections,
            in Location location)
        {
            Selections = selections;
            Location = location;
        }
    }
}