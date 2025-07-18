using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;

using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public class VariableCoercionFacts
{
    private readonly ISchema _schema;
    private readonly ResolversMap _resolvers;

    public VariableCoercionFacts()
    {
        var sdl = @"
            enum Color {
                RED
                GREEN
                BLUE
            }

            input UserInput {
                name: String!
                age: Int
                email: String
                isActive: Boolean
                score: Float
                color: Color
                tags: [String!]
                metadata: [String]
            }

            input NestedInput {
                user: UserInput!
                count: Int!
            }

            type Query {
                user(id: ID!): String
                userWithInput(input: UserInput!): String
                userWithOptionalInput(input: UserInput): String
                userWithNestedInput(input: NestedInput!): String
                userWithMultipleInputs(input1: UserInput!, input2: UserInput): String
                userWithScalarArgs(
                    stringArg: String!,
                    intArg: Int!,
                    floatArg: Float!,
                    boolArg: Boolean!,
                    idArg: ID!,
                    enumArg: Color!,
                    listArg: [String!]!,
                    optionalArg: String
                ): String
            }
        ";

        _resolvers = new ResolversMap
        {
            {
                "Query", new FieldResolversMap
                {
                    { "user", context => context.ResolveAs("user") },
                    { "userWithInput", context => context.ResolveAs("userWithInput") },
                    { "userWithOptionalInput", context => context.ResolveAs("userWithOptionalInput") },
                    { "userWithNestedInput", context => context.ResolveAs("userWithNestedInput") },
                    { "userWithMultipleInputs", context => context.ResolveAs("userWithMultipleInputs") },
                    { "userWithScalarArgs", context => context.ResolveAs("userWithScalarArgs") }
                }
            }
        };

        _schema = new SchemaBuilder()
            .Add(sdl)
            .Build(_resolvers, _resolvers).Result;
    }

    [Fact]
    public async Task VariableCoercion_ValidScalarTypes_IsValid()
    {
        /* Given */
        var query = @"
            query($id: ID!) {
                user(id: $id)
            }";

        var variables = new Dictionary<string, object>
        {
            { "id", "123" }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_IntToFloat_IsValid()
    {
        /* Given */
        var query = @"
            query($floatArg: Float!) {
                userWithScalarArgs(
                    stringArg: ""test"",
                    intArg: 123,
                    floatArg: $floatArg,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: RED,
                    listArg: [""a"", ""b""]
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "floatArg", 42 } // Int should coerce to Float
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_InvalidIntToString_IsInvalid()
    {
        /* Given */
        var query = @"
            query($stringArg: String!) {
                userWithScalarArgs(
                    stringArg: $stringArg,
                    intArg: 123,
                    floatArg: 1.23,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: RED,
                    listArg: [""a"", ""b""]
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "stringArg", 123 } // Int should not coerce to String
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$stringArg\" of type \"String!\" used in position expecting type \"String!\""));
    }

    [Fact]
    public async Task VariableCoercion_ValidInputObject_IsValid()
    {
        /* Given */
        var query = @"
            query($input: UserInput!) {
                userWithInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", new Dictionary<string, object>
                {
                    { "name", "John" },
                    { "age", 30 },
                    { "email", "john@example.com" },
                    { "isActive", true },
                    { "score", 85.5 },
                    { "color", "RED" },
                    { "tags", new[] { "tag1", "tag2" } }
                }
            }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_MissingRequiredField_IsInvalid()
    {
        /* Given */
        var query = @"
            query($input: UserInput!) {
                userWithInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", new Dictionary<string, object>
                {
                    { "age", 30 }, // Missing required "name" field
                    { "email", "john@example.com" }
                }
            }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$input\" got invalid value"));
    }

    [Fact]
    public async Task VariableCoercion_InvalidFieldType_IsInvalid()
    {
        /* Given */
        var query = @"
            query($input: UserInput!) {
                userWithInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", new Dictionary<string, object>
                {
                    { "name", "John" },
                    { "age", "thirty" }, // Invalid type - should be Int
                    { "email", "john@example.com" }
                }
            }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$input\" got invalid value"));
    }

    [Fact]
    public async Task VariableCoercion_ValidEnum_IsValid()
    {
        /* Given */
        var query = @"
            query($enumArg: Color!) {
                userWithScalarArgs(
                    stringArg: ""test"",
                    intArg: 123,
                    floatArg: 1.23,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: $enumArg,
                    listArg: [""a"", ""b""]
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "enumArg", "BLUE" }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_InvalidEnum_IsInvalid()
    {
        /* Given */
        var query = @"
            query($enumArg: Color!) {
                userWithScalarArgs(
                    stringArg: ""test"",
                    intArg: 123,
                    floatArg: 1.23,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: $enumArg,
                    listArg: [""a"", ""b""]
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "enumArg", "PURPLE" } // Invalid enum value
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$enumArg\" got invalid value"));
    }

    [Fact]
    public async Task VariableCoercion_ValidList_IsValid()
    {
        /* Given */
        var query = @"
            query($listArg: [String!]!) {
                userWithScalarArgs(
                    stringArg: ""test"",
                    intArg: 123,
                    floatArg: 1.23,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: RED,
                    listArg: $listArg
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "listArg", new[] { "item1", "item2", "item3" } }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_ListWithNullItem_IsInvalid()
    {
        /* Given */
        var query = @"
            query($listArg: [String!]!) {
                userWithScalarArgs(
                    stringArg: ""test"",
                    intArg: 123,
                    floatArg: 1.23,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: RED,
                    listArg: $listArg
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "listArg", new object[] { "item1", null, "item3" } } // Null not allowed in [String!]
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$listArg\" got invalid value"));
    }

    [Fact]
    public async Task VariableCoercion_SingleValueToList_IsValid()
    {
        /* Given */
        var query = @"
            query($listArg: [String!]!) {
                userWithScalarArgs(
                    stringArg: ""test"",
                    intArg: 123,
                    floatArg: 1.23,
                    boolArg: true,
                    idArg: ""id123"",
                    enumArg: RED,
                    listArg: $listArg
                )
            }";

        var variables = new Dictionary<string, object>
        {
            { "listArg", "single-item" } // Single value should coerce to list
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_NestedInputObject_IsValid()
    {
        /* Given */
        var query = @"
            query($input: NestedInput!) {
                userWithNestedInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", new Dictionary<string, object>
                {
                    { "user", new Dictionary<string, object>
                        {
                            { "name", "John" },
                            { "age", 30 },
                            { "email", "john@example.com" }
                        }
                    },
                    { "count", 5 }
                }
            }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_NullForRequiredVariable_IsInvalid()
    {
        /* Given */
        var query = @"
            query($input: UserInput!) {
                userWithInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", null } // Null not allowed for required variable
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$input\" of required type \"UserInput!\" was not provided"));
    }

    [Fact]
    public async Task VariableCoercion_NullForOptionalVariable_IsValid()
    {
        /* Given */
        var query = @"
            query($input: UserInput) {
                userWithOptionalInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", null } // Null is allowed for optional variable
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task VariableCoercion_UndefinedVariable_IsInvalid()
    {
        /* Given */
        var query = @"
            query($input: UserInput!) {
                userWithInput(input: $input)
            }";

        var variables = new Dictionary<string, object>(); // Variable not provided

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$input\" of required type \"UserInput!\" was not provided"));
    }

    [Fact]
    public async Task VariableCoercion_ExtraFields_IsInvalid()
    {
        /* Given */
        var query = @"
            query($input: UserInput!) {
                userWithInput(input: $input)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input", new Dictionary<string, object>
                {
                    { "name", "John" },
                    { "age", 30 },
                    { "email", "john@example.com" },
                    { "extraField", "not allowed" } // Extra field not in schema
                }
            }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("Variable \"$input\" got invalid value"));
    }

    [Fact]
    public async Task VariableCoercion_ComplexValidation_IsValid()
    {
        /* Given */
        var query = @"
            query($input1: UserInput!, $input2: UserInput) {
                userWithMultipleInputs(input1: $input1, input2: $input2)
            }";

        var variables = new Dictionary<string, object>
        {
            { "input1", new Dictionary<string, object>
                {
                    { "name", "John" },
                    { "age", 30 },
                    { "email", "john@example.com" },
                    { "isActive", true },
                    { "score", 85.5 },
                    { "color", "GREEN" },
                    { "tags", new[] { "tag1", "tag2" } }
                }
            },
            { "input2", new Dictionary<string, object>
                {
                    { "name", "Jane" },
                    { "age", 25 }
                }
            }
        };

        /* When */
        var result = await ValidateQuery(query, variables);

        /* Then */
        Assert.True(result.IsValid);
    }

    private async Task<ValidationResult> ValidateQuery(string query, Dictionary<string, object>? variables = null)
    {
        var request = new GraphQLRequest 
        { 
            Query = query,
            Variables = variables ?? new Dictionary<string, object>()
        };
        var validator = new Validator(_schema);
        return await validator.Validate(request);
    }
}