using System;
using System.Globalization;
using GraphQLParser.AST;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public class DoubleConverter : IValueConverter
    {
        public object Serialize(object value)
        {
            if (value == null)
                return null;

            return Convert.ToDouble(value, NumberFormatInfo.InvariantInfo);
        }

        public object ParseValue(object input)
        {
            if (input == null)
                return null;

            return Convert.ToDouble(input, NumberFormatInfo.InvariantInfo);
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            if (input.Kind == ASTNodeKind.NullValue)
            {
                return null;
            }

            if (input.Kind == ASTNodeKind.FloatValue || input.Kind == ASTNodeKind.IntValue)
            {
                if (input.Value == null)
                    return null;

                return Convert.ToDouble(input.Value, NumberFormatInfo.InvariantInfo);
            }

            throw new FormatException(
                $"Cannot coerce Long value from '{input.Kind}'");
        }
    }
}