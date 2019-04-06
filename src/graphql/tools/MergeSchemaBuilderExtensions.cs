using tanka.graphql.type;

namespace tanka.graphql.tools
{
    public static class MergeSchemaBuilderExtensions
    {
        public static SchemaBuilder Merge(this SchemaBuilder builder, params ISchema[] schemas)
        {
            MergeTool.Schemas(builder, schemas);
            return builder;
        }
    }
}