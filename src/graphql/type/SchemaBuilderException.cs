using System;

namespace tanka.graphql.type
{
    public class SchemaBuilderException : Exception
    {
        public string TypeName { get; set; }

        public SchemaBuilderException(string typeName, string message, Exception inner = null) : base(message, inner)
        {
            TypeName = typeName;
        }
    }
}