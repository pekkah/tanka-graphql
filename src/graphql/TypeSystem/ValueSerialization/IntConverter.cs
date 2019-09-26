using System;
using System.Globalization;
using GraphQLParser.AST;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public class IntConverter : IValueConverter
    {
        public object Serialize(object value)
        {
            if (value == null)
                return null;

            return Convert.ToInt32(value, NumberFormatInfo.InvariantInfo);
        }

        public object ParseValue(object input)
        {
            if (input == null)
                return null;

            return Convert.ToInt32(input, NumberFormatInfo.InvariantInfo);
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            if (input.Kind == ASTNodeKind.NullValue)
            {
                return null;
            }

            if (input.Kind == ASTNodeKind.IntValue)
            {
                if (input.Value == null)
                    return null;

                return Convert.ToInt32(input.Value);
            }

            throw new FormatException(
                $"Cannot coerce Int value from '{input.Kind}'");
        }
    }
}