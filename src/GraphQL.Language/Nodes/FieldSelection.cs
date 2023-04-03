namespace Tanka.GraphQL.Language.Nodes;

public sealed class FieldSelection : ISelection, INode
{
    public readonly Name Alias;

    public readonly Name AliasOrName;
    public readonly Arguments? Arguments;
    public readonly Name Name;
    public readonly SelectionSet? SelectionSet;

    public FieldSelection(
        in Name alias,
        in Name name,
        Arguments? arguments,
        Directives? directives,
        SelectionSet? selectionSet,
        in Location? location = default)
    {
        Alias = alias;
        Name = name;
        Arguments = arguments;
        Directives = directives;
        SelectionSet = selectionSet;
        Location = location;

        AliasOrName = alias != default ? alias : name;
    }

    public NodeKind Kind => NodeKind.FieldSelection;
    public Directives? Directives { get; }
    public Location? Location { get; }

    public SelectionType SelectionType => SelectionType.Field;
}