using System;
using System.Globalization;
using GraphQLParser.AST;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public class StringConverter : IValueConverter
    {
        public object Serialize(object value)
        {
            if (value == null)
                return null;

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public object ParseValue(object input)
        {
            if (input == null)
                return null;

            return Convert.ToString(input, CultureInfo.InvariantCulture);
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            if (input.Kind == ASTNodeKind.NullValue)
            {
                return null;
            }

            if (input.Kind == ASTNodeKind.StringValue) 
                return input.Value;

            throw new FormatException(
                $"Cannot coerce Long value from '{input.Kind}'");
        }
    }
}