using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

public class DelegateResolverFactoryFacts
{
 
    [Fact]
    public async Task ReturnValue_is_Task()
    {
        /* Given */
        static async Task AsyncResolver(ResolverContext context)
        {
            await Task.Delay(0);
            context.ResolvedValue = true;
        }

        Delegate resolverDelegate = AsyncResolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

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

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.True((bool)context.ResolvedValue);
    }

    [Fact]
    public async Task ReturnValue_is_ValueTask()
    {
        /* Given */
        static async ValueTask AsyncResolver(ResolverContext context)
        {
            await Task.Delay(0);
            context.ResolvedValue = true;
        }

        Delegate resolverDelegate = AsyncResolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

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

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.True((bool)context.ResolvedValue);
    }

    [Fact]
    public async Task ReturnValue_is_void()
    {
        /* Given */
        static void Resolver(ResolverContext context)
        {
            context.ResolvedValue = true;
        }

        Delegate resolverDelegate = Resolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

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

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.True((bool)context.ResolvedValue);
    }

    [Fact]
    public async Task ReturnValue_is_TaskT()
    {
        /* Given */
        static async Task<bool> SyncResolver()
        {
            await Task.Delay(0);
            return true;
        }

        Delegate resolverDelegate = SyncResolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

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

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.True((bool)context.ResolvedValue);
    }
}