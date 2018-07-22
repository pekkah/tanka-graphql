using System;
using fugu.graphql.type;

namespace fugu.graphql.error
{
    public class NullValueForNonNullTypeException : Exception
    {
        public IGraphQLType Type { get; }

        public NullValueForNonNullTypeException(string message, IGraphQLType type)
            :base(message)
        {
            Type = type;
        }
    }
}