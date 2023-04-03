namespace Tanka.GraphQL.Language.Nodes;

public sealed class FragmentSpread : ISelection, INode
{
    public readonly Name FragmentName;

    public FragmentSpread(
        in Name fragmentName,
        Directives? directives,
        in Location? location = default)
    {
        FragmentName = fragmentName;
        Directives = directives;
        Location = location;
    }

    public NodeKind Kind => NodeKind.FragmentSpread;
    public Directives? Directives { get; }
    public Location? Location { get; }

    public SelectionType SelectionType => SelectionType.FragmentSpread;
}