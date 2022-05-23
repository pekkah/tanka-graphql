using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class ResolversBuilderFacts
{
    [Fact]
    public void Supports_ObjectInitializer()
    {
        /* Given */
        /* When */
        var builder = new ResolversBuilder
        {
            {
                "object", "field", builder => builder
                    .Use(next => context => ResolveSync.As("Test"))
            }
        };

        /* Then */
        Assert.True(builder.HasResolver("object", "field"));
    }
}