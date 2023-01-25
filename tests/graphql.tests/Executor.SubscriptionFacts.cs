using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class SubscriptionFacts
{
    private static readonly Random _random = new();

    [Fact]
    public async Task Simple_Scalar()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .ConfigureObject("Query", new())
            .ConfigureObject("Subscription", new()
                {
                    {
                        "count: Int!", b => b.Run(ctx =>
                        {
                            ctx.ResolvedValue = ctx.ObjectValue;
                            return default;
                        })
                    }
                },
                new()
                {
                    {
                        "count: Int!", b => b.Run((ctx, cancellationToken) =>
                        {
                            ctx.ResolvedValue = RandomStream(cancellationToken);

                            async IAsyncEnumerable<object> RandomStream(
                                [EnumeratorCancellation] CancellationToken unsubscribe)
                            {
                                var i = 0;
                                unsubscribe.ThrowIfCancellationRequested();
                                yield return ++i;

                                await Task.Delay(100, cancellationToken);
                                yield return ++i;

                                unsubscribe.ThrowIfCancellationRequested();
                                yield return ++i;

                                yield return ++i;

                                unsubscribe.ThrowIfCancellationRequested();
                                yield return ++i;
                            }

                            return default;
                        })
                    }
                })
            .Build();

        ExecutableDocument query = """
            subscription {
                count
            }
            """;

        /* When */
        var stream = new GraphQL.Executor(schema)
            .Subscribe(new GraphQLRequest
            {
                Document = query
            }, CancellationToken.None);

        /* Then */
        var i = 0;
        await foreach (var er in stream)
        {
            ++i;
            er.ShouldMatchJson($$"""
            {
              "data": {
                "count": {{i}}
              }
            }
            """);
        }

        Assert.Equal(5, i);
    }
}