using System.Globalization;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.ValueSerialization;

public class EnumConverter(EnumDefinition enumDefinition) : IValueConverter
{
    public object? Serialize(object? value)
    {
        if (value == null)
            return null;

        var enumValue = enumDefinition.Values?.SingleOrDefault(v => v.Value.Equals(value));
        return enumValue?.Value.Name.Value;
    }

    public ValueBase SerializeLiteral(object? value)
    {
        var serializedValue = Serialize(value);
        if (serializedValue == null)
            return new NullValue();

        return new EnumValue((string)serializedValue);
    }

    public object? ParseValue(object? input)
    {
        if (input == null)
            return null;

        var stringInput = Convert.ToString(input, CultureInfo.InvariantCulture)
            ?.ToUpperInvariant();

        if (stringInput == null) return null;

        var value = enumDefinition.Values?.SingleOrDefault(v => v.Value.Equals(stringInput));
        return value?.Value.Name;
    }

    public object? ParseLiteral(ValueBase input)
    {
        if (input.Kind == NodeKind.NullValue)
            return null;

        var enumValue = (EnumValue)input;
        var value = enumDefinition.Values?.SingleOrDefault(v => v.Value.Equals(enumValue));
        return value?.Value;
    }
}