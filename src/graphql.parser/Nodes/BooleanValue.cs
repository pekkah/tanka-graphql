namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class BooleanValue : IValue
    {
        public readonly bool Value;
        public readonly Location Location;

        public BooleanValue(
            in bool value,
            in Location location)
        {
            Value = value;
            Location = location;
        }
    }
}