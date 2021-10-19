namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class RootOperationTypeDefinition : INode
    {
        public OperationType OperationType { get; }

        public NamedType NamedType { get; }

        public RootOperationTypeDefinition(
            OperationType operationType,
            NamedType namedType,
            in Location? location = default)
        {
            OperationType = operationType;
            NamedType = namedType;
            Location = location;
        }

        public NodeKind Kind => NodeKind.RootOperationTypeDefinition;

        public Location? Location { get; }
    }
}