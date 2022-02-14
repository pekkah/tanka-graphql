namespace Tanka.GraphQL.Language.Nodes;

public sealed class ObjectField : INode
{
    public readonly Name Name;
    public readonly ValueBase Value;

    public ObjectField(
        in Name name,
        ValueBase value,
        in Location? location = default)
    {
        Name = name;
        Value = value;
        Location = location;
    }

    public NodeKind Kind => NodeKind.ObjectField;
    public Location? Location { get; }
}