using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class SchemaExtensions
    {
        public static ISchema WithQuery(this ISchema schema, ObjectType query)
        {
            if (schema.Query == query)
            {
                return schema;
            }

            return new Schema(query);
        }
    }

    public static class NamedTypeExtensions
    {
        public static INamedType WithName(this INamedType namedType, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (namedType.Name == name) return namedType;

            //todo: move to interface
            if (namedType is ObjectType objectType)
            {
                return new ObjectType(
                    name,
                    new Fields(objectType.Fields),
                    objectType.Meta,
                    objectType.Interfaces);
            }

            throw new NotImplementedException("TODO: This should be part of the INamedType interface");
        }
    }

    public static class ObjectTypeExtensions
    {
        public static ObjectType ExcludeFields(
            this ObjectType objectType,
            params KeyValuePair<string, IField>[] excludedFields)
        {
            if (!excludedFields.Any()) return objectType;

            return objectType.WithFields(
                objectType
                    .Fields
                    .Where(field => !excludedFields.Contains(field))
                    .ToArray()
            );
        }

        public static ObjectType WithFields(
            this ObjectType objectType,
            params KeyValuePair<string, IField>[] fields)
        {
            return new ObjectType(
                objectType.Name,
                new Fields(fields),
                objectType.Meta,
                objectType.Interfaces
            );
        }

        public static ObjectType IncludeFields(
            this ObjectType objectType,
            params KeyValuePair<string, IField>[] includedFields)
        {
            if (!includedFields.Any())
                return objectType;

            return new ObjectType(
                objectType.Name,
                new Fields(objectType.Fields.Concat(includedFields)),
                objectType.Meta,
                objectType.Interfaces
            );
        }
    }
}