namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectField: INode
    {
        public NodeKind Kind => NodeKind.ObjectField;
        public Location? Location {get;}
        public readonly Name Name;
        public readonly Value Value;

        public ObjectField(
            in Name name,
            Value value,
            in Location? location = default)
        {
            Name = name;
            Value = value;
            Location = location;
        }
    }
}