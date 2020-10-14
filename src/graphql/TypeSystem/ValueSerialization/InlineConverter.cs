using System;
using Tanka.GraphQL.Language.Nodes;


namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    public sealed class InlineConverter : IValueConverter
    {
        private readonly Func<ValueBase, object?> _parseLiteral;
        private readonly Func<object?, object?> _parseValue;
        private readonly Func<object?, object?> _serialize;
        private readonly Func<object?, ValueBase> _serializeLiteral;

        public InlineConverter(
            Func<object?, object> serialize,
            Func<object?, ValueBase> serializeLiteral,
            Func<object?, object?> parseValue,
            Func<ValueBase, object?> parseLiteral
        )
        {
            _serialize = serialize ?? throw new ArgumentNullException(nameof(serialize));
            _serializeLiteral = serializeLiteral ?? throw new ArgumentNullException(nameof(serializeLiteral)); 
            _parseValue = parseValue ?? throw new ArgumentNullException(nameof(parseValue));
            _parseLiteral = parseLiteral ?? throw new ArgumentNullException(nameof(parseLiteral));
        }

        public object? Serialize(object? value)
        {
            return _serialize(value);
        }

        public ValueBase SerializeLiteral(object? value)
        {
            return _serializeLiteral(value);
        }

        public object? ParseValue(object? input)
        {
            return _parseValue(input);
        }

        public object? ParseLiteral(ValueBase input)
        {
            return _parseLiteral(input);
        }
    }
}