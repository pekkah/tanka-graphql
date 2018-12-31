using System;
using System.Globalization;
using GraphQLParser.AST;

namespace tanka.graphql.type.converters
{
    public class LongConverter : IValueConverter
    {
        public object Serialize(object value)
        {
            if (value == null)
                return null;

            return Convert.ToInt64(value, NumberFormatInfo.InvariantInfo);
        }

        public object ParseValue(object input)
        {
            if (input == null)
                return null;

            return Convert.ToInt64(input, NumberFormatInfo.InvariantInfo);
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            if (input.Kind == ASTNodeKind.IntValue)
            {
                if (input.Value == null)
                    return null;

                return Convert.ToInt64(input.Value);
            }

            return null;
        }
    }
}