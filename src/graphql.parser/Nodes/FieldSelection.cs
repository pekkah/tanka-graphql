namespace Tanka.GraphQL.Language.Nodes
{
    public class FieldSelection : ISelection
    {
        public readonly Location? Location;
        public readonly Name? Alias;
        public readonly Name Name;
        public readonly SelectionSet? SelectionSet;

        public FieldSelection(
            in Name? alias,
            in Name name,
            in SelectionSet? selectionSet,
            in Location? location)
        {
            Alias = alias;
            Name = name;
            SelectionSet = selectionSet;
            Location = location;
        }

        public SelectionType SelectionType => SelectionType.Field;
    }
}