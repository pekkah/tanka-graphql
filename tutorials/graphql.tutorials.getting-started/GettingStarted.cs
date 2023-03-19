using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;
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
                    "Query", "name", () => "Test"
                }
            }
        });

        // Quickest but least configurable way to execute 
        // queries against the schema is to use static
        // execute method of the Executor
        var result = await Executor.Execute(schema, @"{ name }");

        result.ShouldMatchJson("""
            {
              "data": {
                "name": "Test"
              }
            }
            """);
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

        // build schema with resolvers and directives
        var schema = await builder.Build(new SchemaBuildOptions
        {
            Resolvers = new ResolversMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {
                            "name", () => "Test"
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
                                // we will build a new resolver with a middlware to duplicate the value
                                resolver.Use(next => async context =>
                                {
                                    // We need to first call the original resolver to 
                                    // get the initial value
                                     await next(context);

                                     // context should now have the ResolvedValue set by the original resolver
                                     var resolvedValue = context.ResolvedValue;

                                    // for simplicity we expect value to be string
                                    var initialValue = resolvedValue!.ToString();

                                    // return new value
                                    context.ResolvedValue = $"{initialValue} {initialValue}";
                                }).Run(fieldDefinition.Resolver ?? throw new InvalidOperationException()));
                    }
                }
            }
        });

        // execute the query
        var result = await Executor.Execute(schema, @"{ name }");

        result.ShouldMatchJson("""
             {
                "data": {
                    "name": "Test Test"
                    }
            }
            """);
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
                            "url", () => new Uri("https://localhost/")
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
        var result = await Executor.Execute(schema, @"{ url }");

        result.ShouldMatchJson("""
            {
              "data": {
                "url": "https://localhost/"
              }
            }
            """);
    }
}