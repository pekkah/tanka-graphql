namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class EnumValue : ValueBase, INode
    {
        public override NodeKind Kind => NodeKind.EnumValue;
        public override Location? Location {get;}
        public readonly Name Name;

        public EnumValue(
            in Name name,
            in Location? location = default)
        {
            Name = name;
            Location = location;
        }
    }
}