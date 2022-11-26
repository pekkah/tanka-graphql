using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
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
        schema.AddTypeSystem("""
            type Query {
              version: String!
            }
            """);

        schema.AddResolver(
            "Query",
            "version",
            _ => ResolveSync.As("3.0.0")
        );

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