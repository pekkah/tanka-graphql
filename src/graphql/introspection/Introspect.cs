using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class Introspect
    {
        public static ISchema Schema = new IntrospectionSchema().Build(); 

        public static async Task<ISchema> SchemaAsync(Schema schema)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            var introspectionResolvers = new IntrospectionResolvers2(schema);
            return await SchemaTools.MakeExecutableSchemaAsync(
                Schema,
                introspectionResolvers);
        }
    }
}