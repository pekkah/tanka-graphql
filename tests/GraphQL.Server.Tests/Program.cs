using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Tanka.GraphQL;
using Tanka.GraphQL.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddSingleton<EventAggregator<IEvent>>();

builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        options.AddGeneratedTypes(types =>
        {
            types.AddGlobalTypes();
        });
    });


var app = builder.Build();

app.UseRouting();

app.UseWebSockets();
app.MapTankaGraphQL("/graphql", "Default");
app.Run();


[ObjectType]
public static partial class Subscription
{
    public static async IAsyncEnumerable<IEvent> Events(
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            yield return e;
        }
    }

    public static async IAsyncEnumerable<ErrorEvent> ErrorEvents(
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            if (e is ErrorEvent errorEvent)
            {
                if (errorEvent.ShouldThrow)
                {
                    throw new Exception("Test error during subscription");
                }
                yield return errorEvent;
            }
        }
    }

    public static async IAsyncEnumerable<CancellableEvent> CancelableEvents(
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            if (e is CancellableEvent cancellableEvent)
            {
                if (cancellableEvent.ShouldCancel)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                yield return cancellableEvent;
            }
        }
    }

    public static async IAsyncEnumerable<TimeoutEvent> TimeoutEvents(
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            if (e is TimeoutEvent timeoutEvent)
            {
                if (timeoutEvent.ShouldTimeout)
                {
                    await Task.Delay(TimeSpan.FromSeconds(40), cancellationToken);
                }
                yield return timeoutEvent;
            }
        }
    }

    public static async IAsyncEnumerable<IEvent> SecureEvents(
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        // Simulate authorization failure
        throw new UnauthorizedAccessException("Unauthorized access to secure events");
    }

    public static async IAsyncEnumerable<IEvent> EventsByInput(
        string input,
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            yield return e;
        }
    }

    public static async IAsyncEnumerable<IEvent> SlowEvents(
        [FromServices] EventAggregator<IEvent> events,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            await Task.Delay(TimeSpan.FromSeconds(50), cancellationToken);
            yield return e;
        }
    }
}

[ObjectType]
public partial class MessageEvent: IEvent
{
    public string Id { get; set; }
}

[ObjectType]
public partial class ErrorEvent: IEvent
{
    public string Id { get; set; }
    public bool ShouldThrow { get; set; }
}

[ObjectType]
public partial class CancellableEvent: IEvent
{
    public string Id { get; set; }
    public bool ShouldCancel { get; set; }
}

[ObjectType]
public partial class TimeoutEvent: IEvent
{
    public string Id { get; set; }
    public bool ShouldTimeout { get; set; }
}

[InterfaceType]
public partial interface IEvent
{
    string Id { get; }
}



[ObjectType]
public static partial class Query
{
    public static string Hello() => "Hello World!";
}

public partial class Program
{
}