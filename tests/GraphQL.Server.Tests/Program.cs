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
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var e in events.Subscribe(cancellationToken))
        {
            yield return e;
        }
    }
}

[ObjectType]
public partial class MessageEvent : IEvent
{
    public string Id { get; set; }
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