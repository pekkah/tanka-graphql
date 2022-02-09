using System;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem
{
    public static class SchemaExtensions
    {
        public static T GetRequiredNamedType<T>(this ISchema schema, string name) where T: TypeDefinition
        {
            return schema.GetNamedType(name) as T ??
                   throw new ArgumentOutOfRangeException(nameof(name), $"Schema does not contain a named type '{name}'.");
        }

        public static FieldDefinition GetRequiredField(this ISchema schema, string type, string fieldName)
        {
            return schema.GetField(type, fieldName) ?? 
                   throw new ArgumentOutOfRangeException(nameof(fieldName), $"Schema does not contain a field '{type}.{fieldName}'.");
        }

        public static IValueConverter GetRequiredValueConverter(this ISchema schema, string type)
        {
            return schema.GetValueConverter(type) ??
                   throw new ArgumentOutOfRangeException(nameof(type), $"Schema does not contain a value converter for '{type}'.");

        }
    }
}