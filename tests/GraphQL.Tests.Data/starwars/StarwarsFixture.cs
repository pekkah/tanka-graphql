using System;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tests.Data.Starwars;

public class StarwarsFixture : IDisposable
{
    public void Dispose()
    {
    }

    public Task<ISchema> CreateSchema(Starwars starwars)
    {
        var builder = StarwarsSchema.Create();
        var resolvers = StarwarsResolvers.BuildResolvers(starwars);
        var executable = builder.Build(resolvers);

        return executable;
    }
}