using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public static class SchemaBuilderExtensions
    {
        public static SchemaBuilder ImportIntrospectedSchema(
            this SchemaBuilder builder,
            string introspectionResult)
        {
            var result = IntrospectionParser.Deserialize(introspectionResult);
            var reader = new IntrospectionSchemaReader(builder);
            reader.Read(result);

            return builder;
        }
    }
}