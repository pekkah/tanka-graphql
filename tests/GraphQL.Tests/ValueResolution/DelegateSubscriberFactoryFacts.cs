using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.ValueResolution;

public class DelegateSubscriberFactoryFacts
{

    [Fact]
    public async Task ReturnValue_is_Task()
    {
        /* Given */
        static async Task AsyncSubscriber(SubscriberContext context)
        {
            await Task.Delay(0);
            context.ResolvedValue = AsyncEnumerable.Repeat((object)true, 1);
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
        {
            ObjectDefinition = null,
            ObjectValue = null,
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = new Dictionary<string, object?>(),
            Path = null,
            QueryContext = new QueryContext(),
        };

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.True(await context.ResolvedValue.OfType<bool>().SingleAsync());
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_context_and_a_service_parameter()
    {
        /* Given */
        static async Task AsyncSubscriber(SubscriberContext context, IMyDependency dep1)
        {
            await Task.Delay(0);
            context.ResolvedValue = AsyncEnumerable.Repeat((object)dep1, 1);
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
        {
            ObjectDefinition = null,
            ObjectValue = null,
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = new Dictionary<string, object?>(),
            Path = null,
            QueryContext = new QueryContext()
            {
                RequestServices = new ServiceCollection()
                    .AddSingleton<IMyDependency, MyDependency>()
                    .BuildServiceProvider(),
            }
        };

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.IsAssignableFrom<IMyDependency>(await context.ResolvedValue.OfType<IMyDependency>().SingleAsync());
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_context_and_ObjectValue_parameter()
    {
        /* Given */
        static async Task AsyncSubscriber(SubscriberContext context, object? objectValue)
        {
            await Task.Delay(0);
            context.ResolvedValue = AsyncEnumerable.Repeat((object?)objectValue, 1);
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.Equal("test", await context.ResolvedValue.OfType<string>().SingleAsync());
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_context_and_TypedObjectValue_parameter()
    {
        /* Given */
        static async Task AsyncSubscriber(SubscriberContext context, string? objectValue)
        {
            await Task.Delay(0);
            context.ResolvedValue = AsyncEnumerable.Repeat((object?)objectValue, 1);
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.Equal("test", await context.ResolvedValue.OfType<string>().SingleAsync());
    }

    [Fact]
    public async Task ReturnValue_is_Task_with_all_context_members()
    {
        /* Given */
        bool called = false;

        async Task AsyncSubscriber(
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

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task ReturnValue_is_Task_no_parameters()
    {
        /* Given */
        var called = false;

        Task AsyncSubscriber()
        {
            called = true;
            return Task.CompletedTask;
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task ReturnValue_is_ValueTask()
    {
        /* Given */
        static async ValueTask AsyncSubscriber(SubscriberContext context)
        {
            await Task.Delay(0);
            context.ResolvedValue = AsyncEnumerable.Repeat((object)true, 1);
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.True(await context.ResolvedValue.OfType<bool>().SingleAsync());
    }

    [Fact]
    public async Task ReturnValue_is_ValueTask_no_parameters()
    {
        /* Given */
        var called = false;

        ValueTask AsyncSubscriber()
        {
            called = true;
            return default;
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task ReturnValue_is_ValueTask_with_string_arg1()
    {
        /* Given */
        string? arg1test = null;
        ValueTask AsyncSubscriber(string arg1)
        {
            arg1test = arg1;
            return default;
        }

        Delegate subscriberDelegate = AsyncSubscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
        {
            ObjectDefinition = null,
            ObjectValue = null,
            Field = null,
            Selection = null,
            Fields = null,
            ArgumentValues = new Dictionary<string, object?>()
            {
                ["arg1"] = "test"
            },
            Path = null,
            QueryContext = new QueryContext()
        };

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(arg1test);
        Assert.Equal("test", arg1test);
    }

    [Fact]
    public async Task ReturnValue_is_void()
    {
        /* Given */
        static void Subscriber(SubscriberContext context)
        {
            context.ResolvedValue = AsyncEnumerable.Repeat((object)true, 1);
        }

        Delegate subscriberDelegate = Subscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.True(await context.ResolvedValue.OfType<bool>().SingleAsync());
    }

    [Fact]
    public async Task ReturnValue_is_T()
    {
        /* Given */
        static async IAsyncEnumerable<bool> Subscriber()
        {
            await Task.Delay(0);
            yield return true;
        }

        Delegate subscriberDelegate = Subscriber;

        /* When */
        Subscriber subscriber = DelegateSubscriberFactory.Get(subscriberDelegate);

        /* Then */
        var context = new SubscriberContext
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

        await subscriber(context, CancellationToken.None);

        Assert.NotNull(context.ResolvedValue);
        Assert.True(await context.ResolvedValue.OfType<bool>().SingleAsync());
    }
}