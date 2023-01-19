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
            { "version: String!", context => context.ResolveAs("3.0") }
        }));
    })
    //.AddWebSockets()
    //.AddSignalR()
    ;

var app = builder.Build();

app.MapTankaGraphQL3("/graphql", "schemaName");

/*app.MapTankaGraphQL("/graphql", gql =>
{
    gql.UseSchema("schemaName");
    gql.UseDefaultOperationExecutor();
});
*/
app.Run();