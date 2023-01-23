using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Tanka.GraphQL;
using Tanka.GraphQL.Server;

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

//todo: these should be added by default
builder.Services.AddSingleton<Executor>(p =>
{
    // this ctor allows us to pass just the logger and rest of the dependencies are
    // taken from the QueryContext
    return new Executor(p.GetRequiredService<ILogger<Executor>>());
});

var app = builder.Build();

app.MapTankaGraphQL3("/graphql", "schemaName");

/*app.MapTankaGraphQL("/graphql", gql =>
{
    gql.UseSchema("schemaName");
    gql.UseDefaultOperationExecutor();
});
*/
app.Run();