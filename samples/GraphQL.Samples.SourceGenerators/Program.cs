using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        options.AddGeneratedTypes(types =>
        {
            types.AddQueryController();
            types.AddWorldController();
        });
    });

WebApplication? app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.Run();

[ObjectType]
public static class Query
{
    public static World World() => new World();
}

[ObjectType]
public class World
{
    public string Hello() => "Hello World!";
}