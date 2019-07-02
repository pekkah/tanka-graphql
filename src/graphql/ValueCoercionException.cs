using System;
using tanka.graphql.type;

namespace tanka.graphql
{
    public class ValueCoercionException : Exception
    {
        public IType Type { get; }

        public ValueCoercionException(string message, object value, IType type)
            :base(message)
        {
            Type = type;
            Value = value;
        }

        public object Value { get; set; }
    }
}