using System;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public class AnyScalarConverter : IValueConverter
    {
        public object? Serialize(object value)
        {
            return value;
        }

        public ValueBase SerializeLiteral(object? value)
        {
            throw new NotImplementedException("AnyScalar value cannot be printed");
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