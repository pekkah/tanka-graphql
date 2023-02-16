using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.ValueResolution;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(json =>
{
    json.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// configure services
builder.AddTankaGraphQL3()
    .AddSchema("schemaName", schema =>
    {
        schema.Configure(b =>
        {
            b.ConfigureObject("Query", new()
            {
                { "system: System!", context => context.ResolveAs<object>(new { }) }
            });

            b.ConfigureObject("System", new()
            {
                { "version: String!", context => context.ResolveAs("3.0") }
            });

            b.ConfigureObject("Subscription", new Dictionary<FieldDefinition, Action<ResolverBuilder>>()
            {
                { "counter: Int!", r => r.Run(c => c.ResolveAs(c.ObjectValue)) }
            }, new()
            {
                { "counter(to: Int!): Int!", r => r.Run((c, ct) =>
                {
                    c.ResolvedValue = Wrap(Count(c.GetArgument<int>("to"), ct));
                    return default;
                })}
            });

        });
    })
    .AddHttp()
    .AddWebSockets()
    //.AddSignalR()
    ;

var app = builder.Build();

app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL3("/graphql", "schemaName");

// this allows customization of the pipeline
app.MapTankaGraphQL3("/graphql-custom", gql =>
{
    gql.UseDefaults("schemaName");
});

app.Run();

static async IAsyncEnumerable<object?> Wrap<T>(IAsyncEnumerable<T> source)
{
    await foreach (var o in source)
        yield return o;
}

static async IAsyncEnumerable<int> Count(int to, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    int i = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        yield return ++i;

        if (i == to)
            break;

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}