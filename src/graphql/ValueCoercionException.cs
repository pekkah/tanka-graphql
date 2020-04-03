using System;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL
{
    public class ValueCoercionException : Exception
    {
        public IType Type { get; }

        public ValueCoercionException(string message, object? value, IType type)
            :base(message)
        {
            Type = type;
            Value = value;
        }

        public object? Value { get; set; }
    }
}