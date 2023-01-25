using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Tanka.GraphQL;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Server;
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
        schema.Builder.ValidateOnStart();

        schema.Configure(b => b.ConfigureObject("Query", new()
        {
            { "system: System!", context => context.ResolveAs<object>(new {}) }
        }));

        schema.Configure(b => b.ConfigureObject("System", new()
        {
            { "version: String!", context => context.ResolveAs("3.0") }
        }));
    })
    //.AddWebSockets()
    //.AddSignalR()
    ;

var app = builder.Build();

// this uses the default pipeline
app.MapTankaGraphQL3("/graphql", "schemaName");

// this allows customization of the pipeline
app.MapTankaGraphQL3("/graphql-custom", gql =>
{
    gql.UseSchema("schemaName");
    gql.UseDefaultOperationResolver();
    gql.UseDefaultVariableCoercer();
    gql.UseSelectionSetPipeline(sets =>
    {
        sets.UseSelectionSetExecutor();
    });
});

app.Run();