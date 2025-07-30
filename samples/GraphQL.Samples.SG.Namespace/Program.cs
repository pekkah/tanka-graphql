using Tanka.GraphQL.Samples.SG.Namespace;
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
            // Add all types in current namespace
            types.AddTankaGraphQLSamplesSGNamespaceTypes();
        });
    });

WebApplication app = builder.Build();
app.UseWebSockets();

app.MapTankaGraphQL("/graphql", "Default");
app.MapGraphiQL("/graphql/ui");
app.Run();


namespace Tanka.GraphQL.Samples.SG.Namespace
{
    [ObjectType]
    public static partial class Query
    {
        public static World World() => new();
    }

    [ObjectType]
    public partial class World
    {
        public string Hello([FromArguments] HelloInput input) => $"Hello {input.Name}";

    }

    [InputType]
    public partial class HelloInput
    {
        public string Name { get; set; }
    }


}