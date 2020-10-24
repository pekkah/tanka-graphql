namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Argument : INode
    {
        public NodeKind Kind => NodeKind.Argument;
        public Location? Location {get;}
        public readonly Name Name;
        public readonly ValueBase Value;

        public Argument(
            in Name name,
            ValueBase value,
            in Location? location = default)
        {
            Name = name;
            Value = value;
            Location = location;
        }
    }
}