using System.Globalization;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization;

public class BooleanConverter : IValueConverter
{
    public object? Serialize(object? value)
    {
        if (value == null)
            return null;

        if (value is string str)
        {
            if (string.Equals("0", str, StringComparison.Ordinal))
                return false;

            if (string.Equals("1", str, StringComparison.Ordinal))
                return true;
        }

        return Convert.ToBoolean(value, NumberFormatInfo.InvariantInfo);
    }

    public ValueBase SerializeLiteral(object? value)
    {
        var serializedValue = Serialize(value);
        if (serializedValue == null)
            return new NullValue();

        return new BooleanValue((bool)serializedValue);
    }

    public object? ParseValue(object? input)
    {
        if (input == null)
            return null;

        return Convert.ToBoolean(input, NumberFormatInfo.InvariantInfo);
    }

    public object? ParseLiteral(ValueBase input)
    {
        if (input.Kind == NodeKind.NullValue) return null;

        if (input.Kind == NodeKind.BooleanValue)
        {
            var inputBool = (BooleanValue)input;
            var value = inputBool.Value;

            return value;
        }

        throw new FormatException(
            $"Cannot coerce Boolean value from '{input.Kind}'");
    }
}