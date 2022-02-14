namespace Tanka.GraphQL.Language.Nodes;

public sealed class IntValue : ValueBase, INode
{
    public readonly int Value;

    public IntValue(
        int value,
        in Location? location = default)
    {
        Value = value;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.IntValue;
    public override Location? Location { get; }
}