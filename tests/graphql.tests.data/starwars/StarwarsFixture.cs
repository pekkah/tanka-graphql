using System;
using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql.tests.data.starwars
{
    public class StarwarsFixture : IDisposable
    {
        public void Dispose()
        {
        }

        public ISchema CreateSchema(Starwars starwars)
        {
            var schema = StarwarsSchema.Create();
            var resolvers = StarwarsResolvers.BuildResolvers(starwars);
            var executable = SchemaTools.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers);

            return executable;
        }
    }
}