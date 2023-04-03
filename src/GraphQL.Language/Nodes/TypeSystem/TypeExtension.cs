namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class TypeExtension : INode
{
    public TypeExtension(
        TypeDefinition definition,
        in Location? location = default)
    {
        Definition = definition;
        Location = location;
    }

    public TypeDefinition Definition { get; }

    public NodeKind ExtendedKind => Definition.Kind;

    public Name Name => Definition.Name;

    public NodeKind Kind => NodeKind.TypeExtension;

    public Location? Location { get; }
}