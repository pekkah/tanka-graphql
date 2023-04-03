using System.Text;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class FragmentDefinition : INode
{
    public readonly Directives? Directives;
    public readonly Name FragmentName;
    public readonly SelectionSet SelectionSet;
    public readonly NamedType TypeCondition;

    public FragmentDefinition(
        in Name fragmentName,
        NamedType typeCondition,
        Directives? directives,
        SelectionSet selectionSet,
        in Location? location = default)
    {
        FragmentName = fragmentName;
        TypeCondition = typeCondition;
        Directives = directives;
        SelectionSet = selectionSet;
        Location = location;
    }

    public NodeKind Kind => NodeKind.FragmentDefinition;
    public Location? Location { get; }

    public static implicit operator FragmentDefinition(string value)
    {
        var parser = Parser.Create(value);
        return parser.ParseFragmentDefinition();
    }
}