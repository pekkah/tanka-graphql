using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        // This extension point is used by the generator to add
        // type controllers
        options.AddGeneratedTypes(types =>
        {
            // Add generated controllers
            types
                .AddWorldController()
                .AddQueryController();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");
app.Run();

/// <summary>
///     Root query type by naming convention
///     <remarks>
///         We define it as static class so that the generator does not try
///         to use the initialValue as the source of it.
///     </remarks>
/// </summary>
[ObjectType]
public static class Query
{
    public static World World() => new();
}

[ObjectType]
public class World
{
    /// <summary>
    ///     Simple field with one string argument and string return type
    /// </summary>
    /// <param name="name">name: String!</param>
    /// <returns>String!</returns>
    public string Hello(string name) => $"Hello {name}";

    /// <summary>
    ///     This is the async version of the Hello method
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<string> HelloAsync(string name) => await Task.FromResult($"Hello {name}");
}