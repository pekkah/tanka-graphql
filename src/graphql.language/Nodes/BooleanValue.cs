namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class BooleanValue : IValue
    {
        public readonly Location? Location;
        public readonly bool Value;

        public BooleanValue(
            bool value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}