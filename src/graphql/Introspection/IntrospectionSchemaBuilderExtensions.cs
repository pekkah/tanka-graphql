using System;
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.Introspection
{
    public static class IntrospectionSchemaBuilderExtensions
    {
        public static SchemaBuilder ImportIntrospectedSchema(
            this SchemaBuilder builder,
            string introspectionExecutionResultJson)
        {
            if (string.IsNullOrWhiteSpace(introspectionExecutionResultJson))
                throw new ArgumentNullException(nameof(introspectionExecutionResultJson));

            var result = IntrospectionParser.Deserialize(introspectionExecutionResultJson);
            var reader = new IntrospectionSchemaReader(builder, result);
            reader.Read();

            return builder;
        }
    }
}