using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.schema;
using tanka.graphql.sdl;
using Xunit;

namespace tanka.graphql.tutorials.gettingStarted
{
    public class GettingStarted
    {
        [Fact]
        public void Part1_CreateSchema()
        {
            // 1. Create builder
            var builder = new SchemaBuilder();

            // 2. Create Query type from SDL by defining a type with 
            // name Query. This is by convention.
            builder.Sdl(@"
                type Query {
                    name: String
                }
                ");

            // 3. Build schema
            var schema = builder.Build();

            // Assert that created schema matches the inputs
            // note: By convention the name on the Query root type is Query
            Assert.Equal("Query", schema.Query.Name);

            // Schema has methods for querying the fields of the type.
            // In this case assertion is done to check that "name" field exists
            // for Query type. Fields are key value pairs where key is the name
            // of the field and value is the actual field.
            Assert.Single(
                schema.GetFields(schema.Query.Name),
                fieldDef => fieldDef.Key == "name");
        }

        [Fact]
        public async Task Part2_BindResolvers_Manual()
        {
            // Create builder and load sdl
            var builder = new SchemaBuilder()
                .Sdl(@"
                type Query {
                    name: String
                }
                ");

            // Get query type
            builder.GetQuery(out var query);

            // Connections are used to defined fields and resolvers.
            // Connections method can be called multiple times.
            builder.Connections(connections =>
            {
                // Get or add resolver builder for Query.name field
                var nameResolverBuilder = connections
                    .GetOrAddResolver(query, "name");

                // "Run" allows us to define an end of the resolver 
                // chain. You can add "middlewares" using "Use".
                nameResolverBuilder
                    .Run(context =>
                    {
                        // Create result using Test as the value
                        var result = Resolve.As("Test");

                        // Resolvers can be sync or async so
                        // ValueTask result is used to reduce 
                        // allocations
                        return new ValueTask<IResolveResult>(result);
                    });
            });

            // Build schema with the resolver
            var schema = builder.Build();

            // Get resolver for Query.name field
            var nameResolver = schema.GetResolver(schema.Query.Name, "name");

            // Execute the resolver. This is normally handled by the executor.
            var nameValue = await nameResolver(null);
            Assert.Equal("Test", nameValue.Value);
        }
    }
}