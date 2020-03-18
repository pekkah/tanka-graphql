namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class StringValue: IValue
    {
        public readonly string Value;
        public readonly Location Location;

        public StringValue(
            string value,
            in Location location)
        {
            Value = value;
            Location = location;
        }

        public static implicit operator StringValue(string value)
        {
            return new StringValue(value, default);
        }

        public static implicit operator string(StringValue value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            var location = Location.Equals(default) ? Location.ToString() : string.Empty;
            return $"\"{Value}\"{location}";
        }
    }

}