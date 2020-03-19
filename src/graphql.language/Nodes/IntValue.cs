namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class IntValue : IValue
    {
        public readonly Location? Location;
        public readonly int Value;

        public IntValue(
            int value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}