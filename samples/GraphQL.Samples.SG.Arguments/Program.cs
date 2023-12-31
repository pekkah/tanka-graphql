using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add tanka graphql
builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchemaOptions("Default", options =>
    {
        options.AddGeneratedTypes(types =>
        {
            // add types in current namespace
            types.AddGlobalTypes();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");
app.Run();


[ObjectType]
public static class Query
{
    /// <summary>
    ///     Resolver with two primitive arguments
    /// </summary>
    /// <param name="start">Bound automatically from args</param>
    /// <param name="count">Bound automatically from args</param>
    /// <returns></returns>
    public static int[] Range(int start, int count)
    {
        return Enumerable.Range(start, count).ToArray();
    }

    /// <summary>
    ///     Resolver with input object argument
    /// </summary>
    /// <param name="options">todo: bug - requires [FromArguments] in combination with [InputObject]</param>
    /// <returns></returns>
    public static int[] RangeWithInputObject([FromArguments]QueryOptions options)
    {
        return Enumerable.Range(options.Start, options.Count).ToArray();
    }
}


[InputType]
public class QueryOptions
{
    public int Start { get; set; } = 0;

    public int Count { get; set; } = 100;
}