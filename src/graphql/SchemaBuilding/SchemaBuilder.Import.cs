using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
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