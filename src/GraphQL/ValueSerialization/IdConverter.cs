using System.Globalization;
using System.Text;

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueSerialization;

public class IdConverter : IValueConverter
{
    public object? Serialize(object? value)
    {
        if (value == null)
            return null;

        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    public ValueBase SerializeLiteral(object? value)
    {
        var serializedValue = Serialize(value);
        if (serializedValue == null)
            return new NullValue();

        return new StringValue(Encoding.UTF8.GetBytes((string)serializedValue));
    }

    public object? ParseValue(object? input)
    {
        if (input == null)
            return null;

        return Convert.ToString(input, CultureInfo.InvariantCulture);
    }

    public object? ParseLiteral(ValueBase input)
    {
        if (input.Kind == NodeKind.NullValue) return null;

        if (input.Kind == NodeKind.StringValue)
            return (StringValue)input.ToString();

        throw new FormatException(
            $"Cannot coerce Id value from '{input.Kind}'");
    }
}