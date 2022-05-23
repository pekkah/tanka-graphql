using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Tests.Data;
using Xunit;

namespace Tanka.GraphQL.Server.Tests.Usages;

public class GraphQLRequestPipelineBuilderFacts
{
    public GraphQLRequestPipelineBuilderFacts()
    {
        Services = new ServiceCollection()
            .BuildServiceProvider();
    }

    public ServiceProvider Services { get; set; }

    [Fact]
    public async Task Streams()
    {
        /* Given */
        var results = new[]
        {
            new ExecutionResult(),
            new ExecutionResult()
        }.AsAsyncEnumerable();

        var pipe = new GraphQLRequestPipelineBuilder(Services)
            .Use(next => _ =>
            {
                return HelloWorld(next(_));

                static async IAsyncEnumerable<ExecutionResult> HelloWorld(IAsyncEnumerable<ExecutionResult> source)
                {
                    await foreach (var r in source)
                    {
                        r.Data = new Dictionary<string, object>()
                        {
                            ["Hello"] = "World"
                        };

                        yield return r;
                    }

                }
            })
            .Use(next => _ => results)
            .Build();

        /* When */
        var actualStream = pipe(new GraphQLRequestContext());

        /* Then */
        int i = 0;
        await foreach (var actualItem in actualStream)
        {
            i++;
            actualItem.ShouldMatchJson(@"{
  ""data"": {
    ""hello"": ""World""
  },
  ""extensions"": null,
  ""errors"": null
}");
        }

        Assert.Equal(2, i);
    }
}