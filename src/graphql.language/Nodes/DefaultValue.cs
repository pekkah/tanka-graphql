namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class DefaultValue: INode
    {
        public NodeKind Kind => NodeKind.DefaultValue;
        public Location? Location {get;}
        public readonly Value Value;

        public DefaultValue(
            Value value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}