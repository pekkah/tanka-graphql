using System;
using fugu.graphql.type;

namespace fugu.graphql.error
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