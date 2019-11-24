using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tools
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