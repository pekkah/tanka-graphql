using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tutorials.GettingStarted;

public class GettingStarted
{
    [Fact]
    public async Task Part1_CreateSchema()
    {
        // 1. Create builder
        var builder = new SchemaBuilder();

        // 2. Create Query type from SDL by defining a type with 
        // name Query. This is by convention.
        builder.Add(@"
                type Query {
                    name: String
                }
                ");

        // 3. Build schema
        var schema = await builder.Build(new SchemaBuildOptions());

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
            .Add(@"
                type Query {
                    name: String
                }
                ");

        // Build schema with the resolver
        var schema = await builder.Build(new SchemaBuildOptions
        {
            Resolvers = new ResolversMap
            {
                {
                    "Query", "name", context =>
                    {
                        // Create result using Test as the value
                        var result = Resolve.As("Test");

                        // Resolvers can be sync or async so
                        // ValueTask result is used to reduce 
                        // allocations
                        return new ValueTask<IResolverResult>(result);
                    }
                }
            }
        });

        // Get resolver for Query.name field
        var nameResolver = schema.GetResolver(schema.Query.Name, "name")!;

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
            .Add(@"
                    directive @duplicate on FIELD_DEFINITION

                    type Query {
                        name: String @duplicate                
                    }
                    ");

        // todo: This is not currently working and resolvers are being modified but
        // those modified resolvers are not taken into use by anything
        // build schema with resolvers and directives
        var schema = await builder.Build(new SchemaBuildOptions
        {
            Resolvers = new ResolversMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {
                            "name", context =>
                            {
                                var result = Resolve.As("Test");
                                return new ValueTask<IResolverResult>(result);
                            }
                        }
                    }
                }
            },
            DirectiveVisitorFactories = new Dictionary<string, CreateDirectiveVisitor>
            {
                // Apply directives to schema by providing a visitor which
                // will transform the fields with the directive into new
                // fields with the directive logic. Note that the original
                // field will be replaced.
                ["duplicate"] = _ => new DirectiveVisitor
                {
                    // Visitor will visit field definitions
                    FieldDefinition = (directive, fieldDefinition) =>
                    {
                        // We will create new field definition with 
                        // new resolver. New resolver will execute the 
                        // original resolver, duplicate value and return it
                        return fieldDefinition
                            .WithResolver(resolver =>
                                resolver.Use(next => async context =>
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
            }
        });

        // Get resolver for Query.name field
        var nameResolver = schema.GetResolver(schema.Query.Name, "name")!;

        // Execute the resolver. This is normally handled by the executor.
        var nameValue = await nameResolver(null);
        Assert.Equal("TestTest", nameValue.Value);
    }

    [Fact]
    public async Task Part6_Custom_Scalar()
    {
        // Create builder and load sdl
        var builder = new SchemaBuilder()
            .Add(@"

                # Custom scalar defined in the SDL
                scalar Uri

                type Query {
                    url: Uri!
                }
                ");

        // Build schema by binding resolvers from ObjectTypeMap
        var schema = await builder.Build(new SchemaBuildOptions
        {
            Resolvers = new ResolversMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {
                            "url", context => ResolveSync.As(new Uri("https://localhost/"))
                        }
                    }
                }
            },
            ValueConverters = new Dictionary<string, IValueConverter>
            {
                // this will add value converter for Uri scalar type
                ["Uri"] = new InlineConverter(
                    value =>
                    {
                        var uri = (Uri)value;
                        return uri.ToString();
                    },
                    parseValue: value => new Uri(value.ToString()),
                    parseLiteral: value =>
                    {
                        if (value.Kind == NodeKind.StringValue) return new Uri((StringValue)value);

                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            $"Cannot coerce Uri from value kind: '{value.Kind}'");
                    },
                    serializeLiteral: value => new StringValue(Encoding.UTF8.GetBytes(value.ToString())))
            }
        });


        // execute query
        var result = await Executor.ExecuteAsync(new ExecutionOptions
        {
            Schema = schema,
            Document = @"{ url }"
        });

        var url = result.Data["url"];
        Assert.Equal("https://localhost/", url.ToString());
    }
}