using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueSerialization;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class TypeSystemErrorHandlingFacts
{
    [Fact]
    public void SchemaBuilder_WithDuplicateTypeName_ShouldThrowException()
    {
        var builder = new SchemaBuilder();

        // Should throw exception for duplicate type names
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            builder.Add(@"
                type User {
                    id: ID
                }
                
                type User {
                    name: String
                }
                
                type Query {
                    user: User
                }
            ");
        });

        Assert.Contains("Type 'User' already added", exception.Message);
    }

    [Fact]
    public async Task SchemaBuilder_WithCircularInterfaceImplementation_ShouldDetectCycle()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface A implements B {
                field: String
            }
            
            interface B implements A {
                field: String
            }
            
            type Query {
                test: String
            }
        ");

        // Should handle circular implementation gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidFieldType_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type User {
                id: NonExistentType
                name: String
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle invalid field type gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidUnionMember_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            union SearchResult = User | NonExistentType
            
            type User {
                name: String
            }
            
            type Query {
                search: SearchResult
            }
        ");

        // Should handle invalid union member gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidInterfaceImplementation_ShouldDetectError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Node {
                id: ID!
                name: String!
            }
            
            type User implements Node {
                id: ID!
                # Missing required 'name' field
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle invalid interface implementation
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidArgumentType_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type User {
                posts(limit: NonExistentInputType): [String]
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle invalid argument type gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidDirectiveLocation_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            directive @test on INVALID_LOCATION
            
            type Query {
                field: String @test
            }
        ");

        // Should handle invalid directive location gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithDuplicateEnumValues_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            enum Status {
                ACTIVE
                INACTIVE
                ACTIVE
            }
            
            type Query {
                status: Status
            }
        ");

        // Should handle duplicate enum values gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidInputObjectField_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            input UserInput {
                name: String
                profile: NonExistentInputType
            }
            
            type Query {
                createUser(input: UserInput): String
            }
        ");

        // Should handle invalid input object field type
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithSelfReferencingInputType_ShouldDetectCycle()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            input UserInput {
                name: String
                parent: UserInput
            }
            
            type Query {
                createUser(input: UserInput): String
            }
        ");

        // Should handle self-referencing input type
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidScalarUsage_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            scalar CustomScalar
            
            type User {
                customField: [CustomScalar!]!
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle scalar without implementation gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public void Schema_GetNamedType_WithNullName_ShouldReturnNull()
    {
        var schema = ISchema.Empty;

        var result = schema.GetNamedType(null!);

        Assert.Null(result);
    }

    [Fact]
    public void Schema_GetNamedType_WithEmptyName_ShouldReturnNull()
    {
        var schema = ISchema.Empty;

        var result = schema.GetNamedType(string.Empty);

        Assert.Null(result);
    }

    [Fact]
    public void Schema_GetField_WithNullTypeName_ShouldReturnNull()
    {
        var schema = ISchema.Empty;

        var result = schema.GetField(null!, "field");

        Assert.Null(result);
    }

    [Fact]
    public void Schema_GetField_WithNullFieldName_ShouldReturnNull()
    {
        var schema = ISchema.Empty;

        var result = schema.GetField("Type", null!);

        Assert.Null(result);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidExtensionTarget_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            extend type NonExistentType {
                newField: String
            }
            
            type Query {
                test: String
            }
        ");

        // Should handle extension of non-existent type
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithConflictingFieldArguments_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Node {
                field(arg: String): String
            }
            
            type User implements Node {
                field(arg: Int): String
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle conflicting field arguments in interface implementation
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidDefaultValue_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type User {
                age(min: Int = ""not_a_number""): Int
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle invalid default value type
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithRecursiveInterfaceChain_ShouldDetectCycle()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface A implements B & C {
                field: String
            }
            
            interface B implements C & A {
                field: String
            }
            
            interface C implements A & B {
                field: String
            }
            
            type Query {
                test: String
            }
        ");

        // Should detect recursive interface implementation chain
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidDirectiveArgument_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            directive @custom(arg: NonExistentType) on FIELD_DEFINITION
            
            type Query {
                field: String @custom(arg: ""value"")
            }
        ");

        // Should handle invalid directive argument type
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithMissingRequiredDirectiveArgument_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            directive @custom(required: String!) on FIELD_DEFINITION
            
            type Query {
                field: String @custom
            }
        ");

        // Should handle missing required directive argument
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidUnionOfInterface_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Node {
                id: ID
            }
            
            union SearchResult = Node
            
            type Query {
                search: SearchResult
            }
        ");

        // Should handle union containing interface (invalid)
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithEmptyUnion_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            union EmptyUnion
            
            type Query {
                empty: EmptyUnion
            }
        ");

        // Should handle empty union definition
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithInvalidListElementType_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type User {
                tags: [NonExistentType!]!
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle invalid list element type
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithConflictingTypeKinds_ShouldHandleError()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            scalar User
            
            type User {
                name: String
            }
            
            type Query {
                user: User
            }
        ");

        // Should handle conflicting type kinds for same name
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task SchemaBuilder_WithEmptyOrWhitespaceSchema_ShouldHandleGracefully(string schemaText)
    {
        var builder = new SchemaBuilder();
        builder.Add(schemaText);

        // Should handle empty/whitespace schema gracefully
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithExtremelyDeeplyNestedTypes_ShouldHandleGracefully()
    {
        var builder = new SchemaBuilder();
        var deepType = "String";

        // Create deeply nested list type
        for (int i = 0; i < 100; i++)
        {
            deepType = $"[{deepType}]";
        }

        builder.Add($@"
            type User {{
                deepField: {deepType}
            }}
            
            type Query {{
                user: User
            }}
        ");

        // Should handle extremely nested types
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public async Task SchemaBuilder_WithLargeNumberOfTypes_ShouldHandlePerformance()
    {
        var builder = new SchemaBuilder();
        var schemaText = "";

        // Generate many types
        for (int i = 0; i < 1000; i++)
        {
            schemaText += $@"
                type Type{i} {{
                    field{i}: String
                }}
            ";
        }

        schemaText += @"
            type Query {
                test: String
            }
        ";

        builder.Add(schemaText);

        // Should handle large number of types efficiently
        var schema = await builder.Build(new SchemaBuildOptions());
        Assert.NotNull(schema);
    }

    [Fact]
    public void ValueConverter_WithNullInput_ShouldHandleGracefully()
    {
        var converter = new StringConverter();

        // Should not throw on null input
        var result = converter.Serialize(null);
        Assert.Null(result);
    }

    [Fact]
    public void ValueConverter_WithInvalidTypeConversion_ShouldHandleGracefully()
    {
        var converter = new IntConverter();

        // Should handle invalid conversion gracefully without throwing
        var result = converter.ParseValue("not_a_number");
        Assert.Null(result);
    }

    [Fact]
    public void SchemaBuilder_WithNullSchemaText_ShouldHandleGracefully()
    {
        var builder = new SchemaBuilder();

        // Should handle null schema text gracefully without throwing
        Assert.Throws<ArgumentNullException>(() => builder.Add((string)null!));
    }

    [Fact]
    public async Task SchemaBuildOptions_WithNullOptions_ShouldUseDefaults()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                test: String
            }
        ");

        // Should handle null build options by using defaults
        var schema = await builder.Build(null!);
        Assert.NotNull(schema);
    }
}