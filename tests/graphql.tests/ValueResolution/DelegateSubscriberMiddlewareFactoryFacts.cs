using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

public class DelegateSubscriberMiddlewareFactoryFacts
{
    [Fact]
    public async Task Middleware_Next()
    {
        /* Given */
        static async Task Middleware(SubscriberContext context, Subscriber next, CancellationToken unsubscribe)
        {
            await next(context, unsubscribe);
        }

        Delegate middlewareDelegate = Middleware;

        /* When */
        var middleware = DelegateSubscriberMiddlewareFactory.Get(middlewareDelegate);

        /* Then */
        var context = new SubscriberContext()
        {
            ObjectDefinition = null,
            ObjectValue = null,
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = null,
            Path = null,
            QueryContext = null
        };

        await middleware(context, (r, _) =>
        {
            r.ResolvedValue = AsyncEnumerable.Repeat<object?>(true, 1);
            return default;
        }, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.True((bool?)await context.ResolvedValue.SingleAsync(CancellationToken.None));
    }
}