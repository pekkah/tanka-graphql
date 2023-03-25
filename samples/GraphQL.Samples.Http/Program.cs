using System.Runtime.CompilerServices;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// configure services
builder.AddTankaGraphQL3()
    // add named schema
    .AddSchema("System", schema =>
    {
        schema.Add("Query", new FieldsWithResolvers
        {
            { "system: System!", () => new { } }
        });

        schema.Add("System", new FieldsWithResolvers
        {
            { "version: String!", () => "3.0" }
        });

        schema.Add("Subscription", new FieldsWithResolvers
            {
                { "counter: Int!", (int objectValue) => objectValue }
            },
            new FieldsWithSubscribers
            {
                {
                    "counter(to: Int!): Int!", (SubscriberContext c, CancellationToken ct) 
                        => Count(c.GetArgument<int>("to"), ct)
                }
            });
    })
    .AddHttp()
    .AddWebSockets();

WebApplication? app = builder.Build();

// this is required by the websocket transport
app.UseWebSockets();

// this uses the default pipeline
app.MapTankaGraphQL3("/graphql", "System");

// this allows customization of the pipeline
app.MapTankaGraphQL3("/graphql-custom", gql =>
{
    gql.SetProperty("TraceEnabled", app.Environment.IsDevelopment());
    gql.UseDefaults("System");
});

app.Run();

// simple subscriber generating numbers from 0 to the given number
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