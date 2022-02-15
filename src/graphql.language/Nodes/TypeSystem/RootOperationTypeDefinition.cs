namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class RootOperationTypeDefinition : INode
{
    public RootOperationTypeDefinition(
        OperationType operationType,
        NamedType namedType,
        in Location? location = default)
    {
        OperationType = operationType;
        NamedType = namedType;
        Location = location;
    }

    public NamedType NamedType { get; }
    public OperationType OperationType { get; }

    public NodeKind Kind => NodeKind.RootOperationTypeDefinition;

    public Location? Location { get; }
}