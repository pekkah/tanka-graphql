using System;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    //todo: should this parse as SelectionSet?
    public class FieldSetScalarConverter : IValueConverter
    {
        public object? Serialize(object value)
        {
            return value as string;
        }

        public ValueBase SerializeLiteral(object? value)
        {
            if (value == null)
                return new NullValue();

            if (!(value is string fields))
                return new NullValue();

            var bytes = Encoding.UTF8.GetBytes(fields);
            return new StringValue(bytes);
        }

        public object? ParseValue(object? input)
        {
            var fields = input as string;
            return fields;
        }

        public object? ParseLiteral(ValueBase input)
        {
            if (input.Kind == NodeKind.StringValue)
            {
                var str = ((StringValue) input).ToString();
                return ParseValue(str);
            }

            throw new FormatException(
                $"Invalid literal value for FieldSet scalar. Expected StringValue but got {input.Kind}");
        }
    }
}