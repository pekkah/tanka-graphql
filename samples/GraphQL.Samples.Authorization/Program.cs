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
                // Simple string field with hard coded resolved value
                { "hello: String!", () => "Hello World"}
            });

        // add Subscription root
        schema.Add(
            "Subscription",
            new FieldsWithResolvers
            {
                // this will resolve the actual resolved value from the produced values
                { "hello: String!", (string objectValue) => objectValue }
            },
            new FieldsWithSubscribers
            {
                // this is our subscription producer
                { "hello: String!", (CancellationToken unsubscribe) =>
                {
                    return Hello(unsubscribe);

                    static async IAsyncEnumerable<string> Hello([EnumeratorCancellation]CancellationToken unsubscribe)
                    {
                        yield return "Hello";
                        await Task.Delay(TimeSpan.FromSeconds(5), unsubscribe);
                        yield return "World";
                    }
                }}
            });
    });

WebApplication? app = builder.Build();

// this is required by the websocket transport
app.UseWebSockets();

// this uses the default pipeline
// you can access GraphiQL at "/graphql/ui"
app.MapTankaGraphQL("/graphql", "System");

app.Run();