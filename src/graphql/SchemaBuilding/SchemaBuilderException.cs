using System;

namespace Tanka.GraphQL.SchemaBuilding
{
    public class SchemaBuilderException : Exception
    {
        public SchemaBuilderException(string typeName, string message, Exception inner = null) : base(message, inner)
        {
            TypeName = typeName;
        }

        public string TypeName { get; set; }
    }
}