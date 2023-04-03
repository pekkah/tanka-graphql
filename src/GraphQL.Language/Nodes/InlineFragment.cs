namespace Tanka.GraphQL.Language.Nodes;

public sealed class InlineFragment : ISelection, INode
{
    public readonly SelectionSet SelectionSet;

    public readonly NamedType? TypeCondition;

    public InlineFragment(
        NamedType? typeCondition,
        Directives? directives,
        SelectionSet selectionSet,
        in Location? location = default)
    {
        TypeCondition = typeCondition;
        Directives = directives;
        SelectionSet = selectionSet;
        Location = location;
    }

    public NodeKind Kind => NodeKind.InlineFragment;
    public Directives? Directives { get; }
    public Location? Location { get; }

    public SelectionType SelectionType => SelectionType.InlineFragment;
}