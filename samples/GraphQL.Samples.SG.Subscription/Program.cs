using System.Runtime.CompilerServices;

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
            types.AddGlobalTypes();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");
app.Run();

[ObjectType]
public static partial class Subscription
{
    /// <summary>
    ///     This is subscription field producing random integers of count between from and to 
    /// </summary>
    /// <returns></returns>
    public static async IAsyncEnumerable<int> Random(int from, int to, int count, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var r = new Random();

        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return r.Next(from, to);
            await Task.Delay(500, cancellationToken);
        }
    }

    /// <summary>
    ///     This is subscription field producing values implementing interface
    /// </summary>
    /// <returns></returns>
    public static async IAsyncEnumerable<IValue> RandomValues(int count, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var r = new Random();

        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next = r.Next(0, 2);
            if (next == 0)
                yield return new IntValue { Value = next };
            else
                yield return new StringValue { Value = next.ToString("F") };

            await Task.Delay(500, cancellationToken);
        }
    }
}

[ObjectType]
public static partial class Query
{
    // this is required as the graphiql will error without a query field
    public static string Hello() => "Hello World!";
}

[InterfaceType]
public partial interface IValue
{
    public string Hello { get; }
}

[ObjectType]
public partial class IntValue : IValue
{
    public required int Value { get; init; }

    public string Hello => GetType().Name;
}

[ObjectType]
public partial class StringValue : IValue
{
    public required string Value { get; init; }

    public string Hello => GetType().Name;
}