using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Language.Nodes;
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
    public async Task ReturnValue_is_Task_with_context_and_a_service_parameter()
    {
        /* Given */
        static async Task AsyncResolver(ResolverContext context, IMyDependency dep1)
        {
            await Task.Delay(0);
            context.ResolvedValue = dep1;
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
            QueryContext = new QueryContext()
            {
                RequestServices = new ServiceCollection()
                    .AddSingleton<IMyDependency, MyDependency>()
                    .BuildServiceProvider()
            }
        };

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.IsAssignableFrom<IMyDependency>(context.ResolvedValue);
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_context_and_ObjectValue_parameter()
    {
        /* Given */
        static async Task AsyncResolver(ResolverContext context, object? objectValue)
        {
            await Task.Delay(0);
            context.ResolvedValue = objectValue;
        }

        Delegate resolverDelegate = AsyncResolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

        /* Then */
        var context = new ResolverContext
        {
            ObjectDefinition = null,
            ObjectValue = "test",
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = null,
            Path = null,
            QueryContext = null
        };

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.Equal("test", context.ResolvedValue);
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_context_and_TypedObjectValue_parameter()
    {
        /* Given */
        static async Task AsyncResolver(ResolverContext context, string? objectValue)
        {
            await Task.Delay(0);
            context.ResolvedValue = objectValue;
        }

        Delegate resolverDelegate = AsyncResolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

        /* Then */
        var context = new ResolverContext
        {
            ObjectDefinition = null,
            ObjectValue = "test",
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = null,
            Path = null,
            QueryContext = null
        };

        await resolver(context);

        Assert.NotNull(context.ResolvedValue);
        Assert.Equal("test", context.ResolvedValue);
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_all_context_members()
    {
        /* Given */
        bool called = false;
        async Task AsyncResolver(
            ObjectDefinition objectDefinition, 
            string objectValue,
            FieldDefinition field,
            FieldSelection selection,
            IReadOnlyCollection<FieldSelection> fields,
            IReadOnlyDictionary<string, object?> argumentValues,
            NodePath path,
            QueryContext queryContext)
        {
            await Task.Delay(0);
            called = true;
        }

        Delegate resolverDelegate = AsyncResolver;

        /* When */
        Resolver resolver = DelegateResolverFactory.Create(resolverDelegate);

        /* Then */
        var context = new ResolverContext
        {
            ObjectDefinition = null,
            ObjectValue = "test",
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = null,
            Path = null,
            QueryContext = null
        };

        await resolver(context);

        Assert.True(called);
    }

    [Fact]
    public async Task ReturnValue_is_Task_no_parameters()
    {
        /* Given */
        var called = false;
        Task AsyncResolver()
        {
            called = true;
            return Task.CompletedTask;
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

        Assert.True(called);
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
    public async Task ReturnValue_is_ValueTask_no_parameters()
    {
        /* Given */
        var called = false;
        ValueTask AsyncResolver()
        {
            called  = true;
            return default;
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

        Assert.True(called);
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
    public async Task ReturnValue_is_void_no_parameters()
    {
        /* Given */
        var called = false;
        void Resolver()
        {
            called = true;
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

        Assert.True(called);
    }

    [Fact(Skip = "todo")]
    public async Task ReturnValue_is_TaskT()
    {
        /* Given */
        static async Task<bool> AsyncResolver()
        {
            await Task.Delay(0);
            return true;
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
}