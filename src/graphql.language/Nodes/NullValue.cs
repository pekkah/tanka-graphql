namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class NullValue : ValueBase, INode
    {
        public override NodeKind Kind => NodeKind.NullValue;
        public override Location? Location {get;}

        public NullValue(
            in Location? location = default)
        {
            Location = location;
        }
    }
}