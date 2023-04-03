namespace Tanka.GraphQL.Language.Nodes;

public sealed class ListType : TypeBase
{
    public readonly TypeBase OfType;

    public ListType(
        TypeBase ofType,
        in Location? location = default)
    {
        OfType = ofType;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.ListType;
    public override Location? Location { get; }
}