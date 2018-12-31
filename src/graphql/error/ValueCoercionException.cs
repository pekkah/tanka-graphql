using System;
using tanka.graphql.type;

namespace tanka.graphql.error
{
    public class ValueCoercionException : Exception
    {
        public IGraphQLType Type { get; }

        public ValueCoercionException(string message, object value, IGraphQLType type)
            :base(message)
        {
            Type = type;
            Value = value;
        }

        public object Value { get; set; }
    }
}