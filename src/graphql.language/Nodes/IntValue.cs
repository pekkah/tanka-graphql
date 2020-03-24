namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class IntValue : Value
    {
        public readonly Location? Location;
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