namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class BooleanValue : Value, INode
    {
        public override NodeKind Kind => NodeKind.BooleanValue;
        public override Location? Location {get;}
        public readonly bool Value;

        public BooleanValue(
            bool value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}