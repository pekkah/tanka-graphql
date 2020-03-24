namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class BooleanValue : Value
    {
        public readonly Location? Location;
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