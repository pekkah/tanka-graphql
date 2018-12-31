using System;
using System.Threading.Tasks;
using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsFixture : IDisposable
    {
        public Schema Schema { get; } = StarwarsSchema.BuildSchema();

        public async Task<ISchema> MakeExecutableAsync(Starwars starwars)
        {
            var resolvers = StarwarsResolvers.BuildResolvers(starwars, Schema);
            var executable = await SchemaTools.MakeExecutableSchemaAsync(Schema, resolvers).ConfigureAwait(false);
            return executable;
        }

        public void Dispose()
        {
        }
    }
}