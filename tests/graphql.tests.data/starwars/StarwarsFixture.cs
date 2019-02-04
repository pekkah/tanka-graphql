using System;
using System.Threading.Tasks;
using tanka.graphql.type;
using static tanka.graphql.tools.SchemaTools;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsFixture : IDisposable
    {
        public async Task<ISchema> MakeExecutableAsync(Starwars starwars)
        {
            var schema = StarwarsSchema.BuildSchema();
            await schema.InitializeAsync();

            var resolvers = StarwarsResolvers.BuildResolvers(starwars, schema);
            var executable = await MakeExecutableSchemaWithIntrospection(
                schema, 
                resolvers).ConfigureAwait(false);

            return executable;
        }

        public void Dispose()
        {
        }
    }
}