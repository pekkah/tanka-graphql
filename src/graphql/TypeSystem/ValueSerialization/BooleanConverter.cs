﻿using System;
using System.Globalization;
using GraphQLParser.AST;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public class BooleanConverter : IValueConverter
    {
        public object Serialize(object value)
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

        public object ParseValue(object input)
        {
            if (input == null)
                return null;

            return Convert.ToBoolean(input, NumberFormatInfo.InvariantInfo);
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            if (input.Kind == ASTNodeKind.NullValue)
            {
                return null;
            }

            if (input.Kind == ASTNodeKind.BooleanValue)
            {
                var value = input.Value;

                if (value == null)
                    return null;

                if (string.Equals("0", value, StringComparison.Ordinal))
                    return false;

                if (string.Equals("1", value, StringComparison.Ordinal))
                    return true;

                return Convert.ToBoolean(input.Value, NumberFormatInfo.InvariantInfo);
            }

            throw new FormatException(
                $"Cannot coerce Boolean value from '{input.Kind}'");
        }
    }
}