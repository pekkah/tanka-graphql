using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class DirectiveProcessingEdgeCasesFacts
{
    [Fact]
    public async Task Directive_Skip_WithTrueCondition_ShouldSkipField()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip(if: true)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.ContainsKey("message"));
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_Skip_WithFalseCondition_ShouldIncludeField()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip(if: false)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("message"));
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_Include_WithTrueCondition_ShouldIncludeField()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @include(if: true)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("message"));
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_Include_WithFalseCondition_ShouldSkipField()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @include(if: false)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.ContainsKey("message"));
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_SkipAndInclude_BothTrue_ShouldSkipField()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip(if: true) @include(if: true)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.ContainsKey("message"));
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_SkipAndInclude_SkipFalseIncludeTrue_ShouldIncludeField()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip(if: false) @include(if: true)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("message"));
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_WithVariable_ShouldEvaluateCorrectly()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($skipMessage: Boolean!) { 
                message @skip(if: $skipMessage)
                fallback
            }";

        var variables = new Dictionary<string, object?> { ["skipMessage"] = true };

        var result = await Executor.Execute(schema, query, variables);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.ContainsKey("message"));
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_OnInlineFragment_ShouldWorkCorrectly()
    {
        var schema = await CreateInterfaceSchema();

        var query = @"{ 
            animals {
                name
                ... on Dog @include(if: true) {
                    breed
                }
                ... on Cat @skip(if: true) {
                    color
                }
            } 
        }";

        var result = await Executor.Execute(schema, query);

        result.ShouldMatchJson(@"{
            ""data"": {
                ""animals"": [
                    {
                        ""name"": ""Buddy"",
                        ""breed"": ""Golden Retriever""
                    },
                    {
                        ""name"": ""Whiskers""
                    }
                ]
            }
        }");
    }

    [Fact]
    public async Task Directive_OnFragmentSpread_ShouldWorkCorrectly()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            ...MessageFragment @include(if: true)
            fallback
        }
        
        fragment MessageFragment on Query {
            message
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("message"));
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_Deprecated_ShouldStillResolveField()
    {
        var schema = await CreateDeprecatedFieldSchema();

        var query = @"{ 
            oldField
            newField
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("old value", result.Data["oldField"]);
        Assert.Equal("new value", result.Data["newField"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_WithInvalidBooleanValue_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip(if: ""not a boolean"")
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Directive_WithMissingRequiredArgument_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Directive_UnknownDirective_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @unknownDirective
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Directive_OnWrongLocation_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        // @deprecated is not allowed on field selections, only field definitions
        var query = @"{ 
            message @deprecated(reason: ""test"")
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Directive_CustomDirective_ShouldBeRecognized()
    {
        var schema = await CreateTestSchema(@"directive @customDirective(value: String!) on FIELD");

        var query = @"{ 
            message @customDirective(value: ""test"")
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Null(result.Errors);
    }


    [Fact]
    public async Task Directive_OnNestedFields_ShouldProcessCorrectly()
    {
        var schema = await CreateNestedSchema();

        var query = @"{ 
            user {
                name
                profile @include(if: true) {
                    bio @skip(if: false)
                    avatar @skip(if: true)
                }
            }
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Null(result.Errors);

        var userData = (Dictionary<string, object?>)result.Data["user"];
        Assert.Equal("John Doe", userData["name"]);

        var profileData = (Dictionary<string, object?>)userData["profile"];
        Assert.True(profileData.ContainsKey("bio"));
        Assert.False(profileData.ContainsKey("avatar"));
    }

    [Fact]
    public async Task Directive_WithComplexVariableExpression_ShouldEvaluateCorrectly()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($includeMessage: Boolean!, $skipFallback: Boolean!) { 
                message @include(if: $includeMessage)
                fallback @skip(if: $skipFallback)
            }";

        var variables = new Dictionary<string, object?>
        {
            ["includeMessage"] = true,
            ["skipFallback"] = false
        };

        var result = await Executor.Execute(schema, query, variables);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("message"));
        Assert.True(result.Data.ContainsKey("fallback"));
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Equal("fallback value", result.Data["fallback"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_WithNullVariable_ShouldHandleGracefully()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($skipMessage: Boolean) { 
                message @skip(if: $skipMessage)
                fallback
            }";

        var variables = new Dictionary<string, object?> { ["skipMessage"] = null };

        var result = await Executor.Execute(schema, query, variables);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Directive_WithDefaultValue_ShouldUseDefault()
    {
        var schema = await CreateTestSchema(@"directive @customDefault(enabled: Boolean = true) on FIELD");

        var query = @"{ 
            message @customDefault
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello World", result.Data["message"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_OnEnumValue_ShouldProcessCorrectly()
    {
        var schema = await CreateEnumDirectiveSchema();

        var query = @"{ 
            status
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("ACTIVE", result.Data["status"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Directive_RepeatedDirective_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @skip(if: true) @skip(if: false)
            fallback
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Directive_OnMultipleRootFields_ShouldProcessCorrectly()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            message @include(if: true)
            fallback @skip(if: false)
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("message"));
        Assert.True(result.Data.ContainsKey("fallback"));
        Assert.Null(result.Errors);
    }

    private async Task<ISchema> CreateTestSchema(string? additionalSdl = null)
    {
        var builder = new SchemaBuilder();
        var baseSdl = @"
            type Query {
                message: String
                fallback: String
            }
        ";

        var sdl = string.IsNullOrEmpty(additionalSdl)
            ? baseSdl
            : $"{additionalSdl}\n{baseSdl}";

        builder.Add(sdl);

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "message", context =>
                {
                    context.ResolvedValue = "Hello World";
                    return ValueTask.CompletedTask;
                }},
                { "fallback", context =>
                {
                    context.ResolvedValue = "fallback value";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateInterfaceSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Animal {
                name: String!
            }

            type Dog implements Animal {
                name: String!
                breed: String!
            }

            type Cat implements Animal {
                name: String!
                color: String!
            }

            type Query {
                animals: [Animal!]!
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "animals", context =>
                {
                    context.ResolvedValue = new object[]
                    {
                        new { name = "Buddy", breed = "Golden Retriever", __typename = "Dog" },
                        new { name = "Whiskers", color = "Orange", __typename = "Cat" }
                    };
                    // Configure type resolver for interface types
                    context.ResolveAbstractType = (interfaceDef, value) =>
                    {
                        var obj = (dynamic)value;
                        var typename = (string)obj.__typename;
                        return context.Schema.GetRequiredNamedType<ObjectDefinition>(typename);
                    };
                    return ValueTask.CompletedTask;
                }}
            },
            ["Dog"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<dynamic>(d => d.name) },
                { "breed", context => context.ResolveAsPropertyOf<dynamic>(d => d.breed) }
            },
            ["Cat"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<dynamic>(c => c.name) },
                { "color", context => context.ResolveAsPropertyOf<dynamic>(c => c.color) }
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateDeprecatedFieldSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                oldField: String @deprecated(reason: ""Use newField instead"")
                newField: String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "oldField", context =>
                {
                    context.ResolvedValue = "old value";
                    return ValueTask.CompletedTask;
                }},
                { "newField", context =>
                {
                    context.ResolvedValue = "new value";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }


    private async Task<ISchema> CreateNestedSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Profile {
                bio: String
                avatar: String
            }

            type User {
                name: String
                profile: Profile
            }

            type Query {
                user: User
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "user", context =>
                {
                    context.ResolvedValue = new { name = "John Doe", profile = new { bio = "Software Developer", avatar = "avatar.jpg" } };
                    return ValueTask.CompletedTask;
                }}
            },
            ["User"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<dynamic>(u => u.name) },
                { "profile", context => context.ResolveAsPropertyOf<dynamic>(u => u.profile) }
            },
            ["Profile"] = new()
            {
                { "bio", context => context.ResolveAsPropertyOf<dynamic>(p => p.bio) },
                { "avatar", context => context.ResolveAsPropertyOf<dynamic>(p => p.avatar) }
            }
        };

        return await builder.Build(resolvers);
    }


    private async Task<ISchema> CreateEnumDirectiveSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            enum Status {
                ACTIVE @deprecated(reason: ""Use ENABLED instead"")
                INACTIVE
                ENABLED
            }

            type Query {
                status: Status
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "status", context =>
                {
                    context.ResolvedValue = "ACTIVE";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

}