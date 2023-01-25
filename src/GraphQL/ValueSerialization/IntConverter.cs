using System.Globalization;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueSerialization;

public class IntConverter : IValueConverter
{
    public object? Serialize(object? value)
    {
        if (value == null)
            return null;

        return Convert.ToInt32(value, NumberFormatInfo.InvariantInfo);
    }

    public ValueBase SerializeLiteral(object? value)
    {
        var serializedValue = Serialize(value);
        if (serializedValue == null)
            return new NullValue();

        return new IntValue((int)serializedValue);
    }

    public object? ParseValue(object? input)
    {
        if (input == null)
            return null;

        return Convert.ToInt32(input, NumberFormatInfo.InvariantInfo);
    }

    public object? ParseLiteral(ValueBase input)
    {
        if (input.Kind == NodeKind.NullValue) return null;

        if (input.Kind == NodeKind.IntValue) return ((IntValue)input).Value;

        throw new FormatException(
            $"Cannot coerce Int value from '{input.Kind}'");
    }
}