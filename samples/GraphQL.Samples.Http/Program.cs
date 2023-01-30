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
    .AddHttp()
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
                { "randomSequence: Int!", r => r.Run(c => c.ResolveAs(c.ObjectValue)) }
            }, new()
            {
                { "randomSequence: Int!", r => r.ResolveAsStream(Random) }
            });

        });
    })
    //.AddWebSockets()
    //.AddSignalR()
    ;

builder.Services.AddSingleton<IGraphQLTransport, GraphQLWSTransport>();

var app = builder.Build();

app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL3("/graphql", "schemaName");

// this allows customization of the pipeline
app.MapTankaGraphQL3("/graphql-custom", gql =>
{
    gql.UseSchema("schemaName");
    gql.UseDefaultOperationResolver();
    gql.UseDefaultVariableCoercer();
    gql.UseDefaultValidator();
    gql.UseDefaultSelectionSetPipeline();
});

app.Run();

static async IAsyncEnumerable<int> Random(CancellationToken cancellationToken)
{
    int i = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        yield return ++i;
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }
}