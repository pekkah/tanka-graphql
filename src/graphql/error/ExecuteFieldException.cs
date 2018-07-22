using System;
using fugu.graphql.type;

namespace fugu.graphql.error
{
    public class ExecuteFieldException : Exception
    {
        public ExecuteFieldException(string message, ComplexType objectType, IField field, Exception inner)
            : base(message, inner)
        {
            ObjectType = objectType;
            Field = field;
        }

        public ComplexType ObjectType { get; }

        public IField Field { get; }
    }
}