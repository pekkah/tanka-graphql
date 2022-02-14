namespace Tanka.GraphQL.Language.Nodes;

public sealed class BooleanValue : ValueBase, INode
{
    public readonly bool Value;

    public BooleanValue(
        bool value,
        in Location? location = default)
    {
        Value = value;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.BooleanValue;
    public override Location? Location { get; }
}