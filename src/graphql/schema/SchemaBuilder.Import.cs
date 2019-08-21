using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql.schema
{
    public partial class SchemaBuilder
    {
        public SchemaBuilder Import(params ISchema[] schemas)
        {
            MergeTool.Schemas(this, schemas);
            return this;
        }
    }
}