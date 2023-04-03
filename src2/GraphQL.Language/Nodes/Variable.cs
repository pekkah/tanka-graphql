namespace Tanka.GraphQL.Language.Nodes;

public sealed class Variable : ValueBase, INode
{
    public readonly Name Name;

    public Variable(
        in Name name,
        in Location? location = default)
    {
        Name = name;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.Variable;
    public override Location? Location { get; }
}