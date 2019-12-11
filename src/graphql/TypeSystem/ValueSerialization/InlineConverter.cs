using System;
using GraphQLParser.AST;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public sealed class InlineConverter : IValueConverter
    {
        private readonly Func<GraphQLScalarValue, object> _parseLiteral;
        private readonly Func<object, object> _parseValue;
        private readonly Func<object, object> _serialize;

        public InlineConverter(
            Func<object, object> serialize,
            Func<object, object> parseValue,
            Func<GraphQLScalarValue, object> parseLiteral
        )
        {
            _serialize = serialize ?? throw new ArgumentNullException(nameof(serialize));
            _parseValue = parseValue ?? throw new ArgumentNullException(nameof(parseValue));
            _parseLiteral = parseLiteral ?? throw new ArgumentNullException(nameof(parseLiteral));
        }

        public object Serialize(object value)
        {
            return _serialize(value);
        }

        public object ParseValue(object input)
        {
            return _parseValue(input);
        }

        public object ParseLiteral(GraphQLScalarValue input)
        {
            return _parseLiteral(input);
        }
    }
}