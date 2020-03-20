using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class StringValue : IValue
    {
        public readonly Location? Location;
        public readonly byte[] Value;

        public StringValue(
            in byte[] value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }

        public static implicit operator StringValue(string value)
        {
            return new StringValue(Encoding.UTF8.GetBytes(value), default);
        }

        public static implicit operator string(StringValue value)
        {
            return Encoding.UTF8.GetString(value.Value);
        }

        public override string ToString()
        {
            var location = Location.Equals(default) ? Location.ToString() : string.Empty;
            return $"\"{Value}\"{location}";
        }
    }
}