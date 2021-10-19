using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class SelectionSet : CollectionNodeBase<ISelection>
    {
        //todo: remove
        public readonly IReadOnlyList<ISelection> Selections;

        public SelectionSet(
            IReadOnlyList<ISelection> selections,
            in Location? location = default) : base(selections, in location)
        {
            Selections = selections;
        }

        public override NodeKind Kind => NodeKind.SelectionSet;
    }
}