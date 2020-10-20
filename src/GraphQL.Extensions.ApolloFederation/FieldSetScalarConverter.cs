using System;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public class FieldSetScalarConverter : IValueConverter
    {
        public object? Serialize(object value)
        {
            return value.ToString();
        }

        public ValueBase SerializeLiteral(object? value)
        {
            var bytes = Encoding.UTF8.GetBytes(value.ToString());
            return new StringValue(bytes);
        }

        public object? ParseValue(object input)
        {
            return input;
        }

        public object? ParseLiteral(ValueBase input)
        {
            return input;
        }
    }
}