namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class IntValue : Value, INode
    {
        public override NodeKind Kind => NodeKind.IntValue;
        public override Location? Location {get;}
        public readonly int Value;

        public IntValue(
            int value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}