using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Directives;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tutorials.GettingStarted
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

        [Fact]
        public async Task Part2_BindResolvers_SchemaBuilder_Maps()
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

            // Bind resolvers from ObjectTypeMap
            builder.UseResolversAndSubscribers(
                new ObjectTypeMap
                {
                    {
                        query.Name, new FieldResolversMap
                        {
                            {
                                "name", context =>
                                {
                                    var result = Resolve.As("Test");
                                    return new ValueTask<IResolveResult>(result);
                                }
                            }
                        }
                    }
                });

            // Build schema
            var schema = builder.Build();

            // Get resolver for Query.name field
            var nameResolver = schema.GetResolver(schema.Query.Name, "name");

            // Execute the resolver. This is normally handled by the executor.
            var nameValue = await nameResolver(null);
            Assert.Equal("Test", nameValue.Value);
        }

        [Fact]
        public async Task Part2_BindResolvers_SchemaTools_Maps()
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

            // Build schema by binding resolvers from ObjectTypeMap
            var schema = SchemaTools.MakeExecutableSchema(
                builder,
                new ObjectTypeMap
                {
                    {
                        query.Name, new FieldResolversMap
                        {
                            {
                                "name", context =>
                                {
                                    var result = Resolve.As("Test");
                                    return new ValueTask<IResolveResult>(result);
                                }
                            }
                        }
                    }
                });


            // Get resolver for Query.name field
            var nameResolver = schema.GetResolver(schema.Query.Name, "name");

            // Execute the resolver. This is normally handled by the executor.
            var nameValue = await nameResolver(null);
            Assert.Equal("Test", nameValue.Value);
        }

        [Fact]
        public async Task Part3_ApplyDirectives_on_Object_fields()
        {
            // Create builder, load sdl with our types
            // and bind the resolvers
            var builder = new SchemaBuilder()
                .Sdl(@"
                    directive @duplicate on FIELD_DEFINITION

                    type Query {
                        name: String @duplicate                
                    }
                    ")
                .GetQuery(out var query)
                .UseResolversAndSubscribers(
                    new ObjectTypeMap
                    {
                        {
                            query.Name, new FieldResolversMap
                            {
                                {
                                    "name", context =>
                                    {
                                        var result = Resolve.As("Test");
                                        return new ValueTask<IResolveResult>(result);
                                    }
                                }
                            }
                        }
                    });

            // Apply directives to schema by providing a visitor which
            // will transform the fields with the directive into new
            // fields with the directive logic. Note that the original
            // field will be replaced.
            builder.ApplyDirectives(new Dictionary<string, CreateDirectiveVisitor>()
            {
                ["duplicate"] = _ => new DirectiveVisitor()
                {
                    // Visitor will visit field definitions
                    FieldDefinition = (directive, fieldDefinition) =>
                    {
                        // We will create new field definition with 
                        // new resolver. New resolver will execute the 
                        // original resolver, duplicate value and return it
                        return fieldDefinition
                            .WithResolver(resolver => 
                                resolver.Use(async (context, next) =>
                        {
                            // We need to first call the original resolver to 
                            // get the initial value
                            var result = await next(context);

                            // for simplicity we expect value to be string
                            var initialValue = result.Value.ToString();

                            // return new value
                            return Resolve.As(initialValue + initialValue);
                        }).Run(fieldDefinition.Resolver));
                    }
                }
            });

            // Build schema
            var schema = builder.Build();

            // Get resolver for Query.name field
            var nameResolver = schema.GetResolver(schema.Query.Name, "name");

            // Execute the resolver. This is normally handled by the executor.
            var nameValue = await nameResolver(null);
            Assert.Equal("TestTest", nameValue.Value);
        }
    }
}