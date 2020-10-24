namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class DefaultValue: INode
    {
        public NodeKind Kind => NodeKind.DefaultValue;
        public Location? Location {get;}
        public readonly ValueBase Value;

        public DefaultValue(
            ValueBase value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}