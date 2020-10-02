﻿using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public class FieldSetScalarConverter : IValueConverter
    {
        public object? Serialize(object value)
        {
            return value;
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