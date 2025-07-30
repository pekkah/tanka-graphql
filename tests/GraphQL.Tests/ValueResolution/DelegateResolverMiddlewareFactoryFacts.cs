using System;
using System.Threading.Tasks;

using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

public class DelegateResolverMiddlewareFactoryFacts
{
    [Fact]
    public async Task Middleware_Next()
    {
        /* Given */
        static async Task Middleware(ResolverContext context, Resolver next)
        {
            await next(context);
        }

        Delegate middlewareDelegate = Middleware;

        /* When */
        var middleware = DelegateResolverMiddlewareFactory.Get(middlewareDelegate);

        /* Then */
        var context = new ResolverContext
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

        await middleware(context, r => r.ResolveAs(true));

        Assert.NotNull(context.ResolvedValue);
        Assert.True((bool)context.ResolvedValue);
    }
}