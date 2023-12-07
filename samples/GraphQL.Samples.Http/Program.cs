using System.Runtime.CompilerServices;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// add required services
builder.AddTankaGraphQL()
    // add http transport
    .AddHttp()
    // add websocket transport for subscriptions
    .AddWebSockets()
    // add named schema
    .AddSchema("System", schema =>
    {
        // add Query root
        schema.Add(
            "Query",
            new FieldsWithResolvers
            {
                // We will just return new object as resolved value
                { "system: System!", () => new SystemDefinition() }
            });

        // Add system type with version field of type String!
        schema.Add(
            "System",
            new FieldsWithResolvers
            {
                // version is resolved from the objectValue (the parent value of type SystemDefinition)
                { "version: String!", (SystemDefinition objectValue) => objectValue.Version }
            });

        // add Subscription root
        schema.Add(
            "Subscription",
            new FieldsWithResolvers
            {
                // this will resolve the actual resolved value from the produced values
                { "counter: Int!", (int objectValue) => objectValue }
            },
            new FieldsWithSubscribers
            {
                // this is our subscription producer
                { "counter(to: Int!): Int!", Count }
            });
    });

WebApplication app = builder.Build();

// this is required by the websocket transport
app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL("/graphql", "System");
// you can access GraphiQL at "/graphql/ui"
app.MapGraphiQL("/graphql/ui");

app.Run();

// simple subscription generating numbers from 0 to the given number
static async IAsyncEnumerable<int> Count(int to, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var i = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        yield return ++i;

        if (i == to)
            break;

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}

public record SystemDefinition(string Version = "3.0");