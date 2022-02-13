using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
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

        public static TypeSystemDocument ToTypeSystem(this ISchema schema)
        {
            var typeDefinitions = schema.QueryTypes<TypeDefinition>().ToList();
            var directiveDefinitions = schema.QueryDirectiveTypes().ToList();

            var schemaDefinition = schema.ToSchemaDefinition();
            return new TypeSystemDocument(
                new []{schemaDefinition},
                typeDefinitions,
                directiveDefinitions);
        }

        public static SchemaDefinition ToSchemaDefinition(this ISchema schema)
        {
            return new SchemaDefinition(
                null,
                new Language.Nodes.Directives(schema.Directives.ToList()),
                new RootOperationTypeDefinitions(GetOperations(schema).ToList()));

            static IEnumerable<RootOperationTypeDefinition> GetOperations(ISchema schema)
            {
                yield return new RootOperationTypeDefinition(
                    OperationType.Query,
                    new NamedType(schema.Query.Name));

                if (schema.Mutation is not null)
                    yield return new RootOperationTypeDefinition(
                        OperationType.Mutation,
                        new NamedType(schema.Mutation.Name));

                if (schema.Subscription is not null)
                    yield return new RootOperationTypeDefinition(
                        OperationType.Mutation,
                        new NamedType(schema.Subscription.Name));
            }
        }
    }
}