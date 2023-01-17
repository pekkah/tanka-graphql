using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Text;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization;

public class DoubleConverter : IValueConverter
{
    public object? Serialize(object? value)
    {
        if (value == null)
            return null;

        return Convert.ToDouble(value, NumberFormatInfo.InvariantInfo);
    }

    public ValueBase SerializeLiteral(object? value)
    {
        var serializedValue = Serialize(value);
        if (serializedValue == null)
            return new NullValue();

        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(double));
        try
        {
            var span = buffer.AsSpan();
            if (Utf8Formatter.TryFormat((double)serializedValue, span, out var bytesWritten))
            {
                var bytes = span.Slice(0, bytesWritten).ToArray();
                return new FloatValue(bytes, false);
            }

            throw new FormatException($"Cannot serialize value of '{value} as double");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public object? ParseValue(object? input)
    {
        if (input == null)
            return null;

        return Convert.ToDouble(input, NumberFormatInfo.InvariantInfo);
    }

    public object? ParseLiteral(ValueBase input)
    {
        if (input.Kind == NodeKind.NullValue) return null;

        if (input.Kind == NodeKind.FloatValue)
        {
            var doubleValue = (FloatValue)input;

            if (!Utf8Parser.TryParse(doubleValue.ValueSpan, out double d, out _))
                throw new FormatException(
                    $"Could not parse value '{Encoding.UTF8.GetString(doubleValue.ValueSpan)}' as double");

            return d;
        }

        if (input.Kind == NodeKind.IntValue)
        {
            var intValue = (IntValue)input;
            return (double)intValue.Value;
        }

        throw new FormatException(
            $"Cannot coerce Float value from '{input.Kind}'");
    }
}