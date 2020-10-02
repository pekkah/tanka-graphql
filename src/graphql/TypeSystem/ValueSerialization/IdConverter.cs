using System;
using System.Globalization;
using Tanka.GraphQL.Language.Nodes;


namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public class IdConverter : IValueConverter
    {
        public object? Serialize(object value)
        {
            if (value == null)
                return null;

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public object? ParseValue(object input)
        {
            if (input == null)
                return null;

            return Convert.ToString(input, CultureInfo.InvariantCulture);
        }

        public object? ParseLiteral(ValueBase input)
        {
            if (input.Kind == NodeKind.NullValue)
            {
                return null;
            }

            if (input.Kind == NodeKind.StringValue) 
                return (StringValue)input.ToString();

            throw new FormatException(
                $"Cannot coerce Id value from '{input.Kind}'");
        }
    }
}