namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class StringValue: Value
    {
        public readonly string Value;
        public readonly Location? Location;

        public StringValue(
            in string value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}