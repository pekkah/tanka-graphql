using System;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tests.Data.Starwars
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