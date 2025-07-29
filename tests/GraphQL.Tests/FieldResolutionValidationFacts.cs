using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class FieldResolutionValidationFacts
{
    [Fact]
    public async Task FieldResolution_WithValidArguments_ShouldResolveCorrectly()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greeting(name: ""World"") 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello, World!", result.Data["greeting"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithMissingRequiredArgument_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greeting 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("required", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FieldResolution_WithWrongArgumentType_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greeting(name: 123) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithOptionalArgument_ShouldUseDefaultValue()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greetingWithDefault 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello, Anonymous!", result.Data["greetingWithDefault"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithOptionalArgumentProvided_ShouldUseProvidedValue()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greetingWithDefault(name: ""Custom"") 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello, Custom!", result.Data["greetingWithDefault"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithNullableArgument_ShouldAcceptNull()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greetingNullable(name: null) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello, Guest!", result.Data["greetingNullable"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithComplexInputObject_ShouldResolveCorrectly()
    {
        var schema = await CreateInputObjectSchema();

        var query = @"{ 
            processUser(input: { name: ""John"", age: 30, email: ""john@example.com"" }) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Processed user: John (30) - john@example.com", result.Data["processUser"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithInputObjectMissingRequiredField_ShouldReturnValidationError()
    {
        var schema = await CreateInputObjectSchema();

        var query = @"{ 
            processUser(input: { age: 30, email: ""john@example.com"" }) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithInputObjectOptionalField_ShouldUseDefault()
    {
        var schema = await CreateInputObjectSchema();

        var query = @"{ 
            processUser(input: { name: ""John"", age: 30 }) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Processed user: John (30) - noemail@example.com", result.Data["processUser"]);
    }

    [Fact]
    public async Task FieldResolution_WithListArgument_ShouldResolveCorrectly()
    {
        var schema = await CreateListArgumentSchema();

        var query = @"{ 
            sumNumbers(numbers: [1, 2, 3, 4, 5]) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(15, result.Data["sumNumbers"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithEmptyListArgument_ShouldReturnZero()
    {
        var schema = await CreateListArgumentSchema();

        var query = @"{ 
            sumNumbers(numbers: []) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data["sumNumbers"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithListArgumentWrongType_ShouldReturnValidationError()
    {
        var schema = await CreateListArgumentSchema();

        var query = @"{ 
            sumNumbers(numbers: [1, ""two"", 3]) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithEnumArgument_ShouldResolveCorrectly()
    {
        var schema = await CreateEnumArgumentSchema();

        var query = @"{ 
            getStatusMessage(status: ACTIVE) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Status is active", result.Data["getStatusMessage"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithInvalidEnumValue_ShouldReturnValidationError()
    {
        var schema = await CreateEnumArgumentSchema();

        var query = @"{ 
            getStatusMessage(status: INVALID_STATUS) 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithNestedField_ShouldResolveCorrectly()
    {
        var schema = await CreateNestedFieldSchema();

        var query = @"{ 
            user { 
                name 
                profile { 
                    bio 
                    avatar 
                } 
            } 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);

        var userData = (Dictionary<string, object?>)result.Data["user"];
        Assert.Equal("John Doe", userData["name"]);

        var profileData = (Dictionary<string, object?>)userData["profile"];
        Assert.Equal("Software Developer", profileData["bio"]);
        Assert.Equal("avatar.jpg", profileData["avatar"]);

        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithNonExistentNestedField_ShouldReturnValidationError()
    {
        var schema = await CreateNestedFieldSchema();

        var query = @"{ 
            user { 
                name 
                profile { 
                    nonExistentField 
                } 
            } 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithInterfaceField_ShouldResolveCorrectly()
    {
        var schema = await CreateInterfaceSchema();

        var query = @"{ 
            animals { 
                name 
                sound 
                ... on Dog { 
                    breed 
                } 
                ... on Cat { 
                    color 
                } 
            } 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Null(result.Errors);

        var animals = (List<object>)result.Data["animals"];
        Assert.Equal(2, animals.Count);
    }

    [Fact]
    public async Task FieldResolution_WithUnionField_ShouldResolveCorrectly()
    {
        var schema = await CreateUnionSchema();

        var query = @"{ 
            searchResult { 
                ... on User { 
                    name 
                    email 
                } 
                ... on Post { 
                    title 
                    content 
                } 
            } 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithResolverThrowingException_ShouldReturnFieldError()
    {
        var schema = await CreateErrorThrowingSchema();

        var query = @"{ 
            throwingField 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Test resolver exception", result.Errors[0].Message);
    }

    [Fact]
    public async Task FieldResolution_WithResolverReturningNull_ShouldHandleGracefully()
    {
        var schema = await CreateNullReturningSchema();

        var query = @"{ 
            nullField 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data["nullField"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithNonNullFieldReturningNull_ShouldReturnError()
    {
        var schema = await CreateNonNullFieldSchema();

        var query = @"{ 
            nonNullField 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Cannot return null", result.Errors[0].Message);
    }

    [Fact]
    public async Task FieldResolution_WithDeprecatedField_ShouldStillResolve()
    {
        var schema = await CreateDeprecatedFieldSchema();

        var query = @"{ 
            deprecatedField 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("This field is deprecated", result.Data["deprecatedField"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithCustomScalarArgument_ShouldResolveCorrectly()
    {
        var schema = await CreateCustomScalarSchema();

        var query = @"{ 
            processDateTime(date: ""2023-01-01T00:00:00Z"") 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldValidation_WithTooManyArguments_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ 
            greeting(name: ""World"", extraArg: ""invalid"") 
        }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithVariableArgument_ShouldResolveCorrectly()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($userName: String!) { 
                greeting(name: $userName) 
            }";

        var variables = new Dictionary<string, object?> { ["userName"] = "Variable User" };

        var result = await Executor.Execute(schema, query, variables);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello, Variable User!", result.Data["greeting"]);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task FieldResolution_WithInvalidVariableType_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($userName: Int!) { 
                greeting(name: $userName) 
            }";

        var variables = new Dictionary<string, object?> { ["userName"] = 123 };

        var result = await Executor.Execute(schema, query, variables);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    private async Task<ISchema> CreateTestSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                greeting(name: String!): String
                greetingWithDefault(name: String = ""Anonymous""): String
                greetingNullable(name: String): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "greeting", context =>
                {
                    var name = (string)context.ArgumentValues["name"];
                    context.ResolvedValue = $"Hello, {name}!";
                    return ValueTask.CompletedTask;
                }},
                { "greetingWithDefault", context =>
                {
                    var name = context.ArgumentValues.TryGetValue("name", out var nameValue)
                        ? (string)nameValue
                        : "Anonymous";
                    context.ResolvedValue = $"Hello, {name}!";
                    return ValueTask.CompletedTask;
                }},
                { "greetingNullable", context =>
                {
                    var name = context.ArgumentValues.TryGetValue("name", out var nameValue) && nameValue != null
                        ? (string)nameValue
                        : "Guest";
                    context.ResolvedValue = $"Hello, {name}!";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateInputObjectSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            input UserInput {
                name: String!
                age: Int!
                email: String = ""noemail@example.com""
            }

            type Query {
                processUser(input: UserInput!): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "processUser", context =>
                {
                    var input = (Dictionary<string, object>)context.ArgumentValues["input"];
                    var name = (string)input["name"];
                    var age = (int)input["age"];
                    var email = input.TryGetValue("email", out var emailValue)
                        ? (string)emailValue
                        : "noemail@example.com";

                    context.ResolvedValue = $"Processed user: {name} ({age}) - {email}";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateListArgumentSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                sumNumbers(numbers: [Int!]!): Int
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "sumNumbers", context =>
                {
                    var numbers = (List<object>)context.ArgumentValues["numbers"];
                    var sum = 0;
                    foreach (var number in numbers)
                    {
                        sum += (int)number;
                    }
                    context.ResolvedValue = sum;
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateEnumArgumentSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            enum Status {
                ACTIVE
                INACTIVE
                PENDING
            }

            type Query {
                getStatusMessage(status: Status!): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "getStatusMessage", context =>
                {
                    var status = (string)context.ArgumentValues["status"];
                    context.ResolvedValue = $"Status is {status.ToLower()}";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateNestedFieldSchema()
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

    private async Task<ISchema> CreateInterfaceSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            interface Animal {
                name: String!
                sound: String!
            }

            type Dog implements Animal {
                name: String!
                sound: String!
                breed: String!
            }

            type Cat implements Animal {
                name: String!
                sound: String!
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
                        new { name = "Buddy", sound = "Woof", breed = "Golden Retriever", __typename = "Dog" },
                        new { name = "Whiskers", sound = "Meow", color = "Orange", __typename = "Cat" }
                    };
                    return ValueTask.CompletedTask;
                }}
            },
            ["Dog"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<dynamic>(d => d.name) },
                { "sound", context => context.ResolveAsPropertyOf<dynamic>(d => d.sound) },
                { "breed", context => context.ResolveAsPropertyOf<dynamic>(d => d.breed) }
            },
            ["Cat"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<dynamic>(c => c.name) },
                { "sound", context => context.ResolveAsPropertyOf<dynamic>(c => c.sound) },
                { "color", context => context.ResolveAsPropertyOf<dynamic>(c => c.color) }
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateUnionSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type User {
                name: String!
                email: String!
            }

            type Post {
                title: String!
                content: String!
            }

            union SearchResult = User | Post

            type Query {
                searchResult: SearchResult
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "searchResult", context =>
                {
                    context.ResolvedValue = new { name = "John Doe", email = "john@example.com", __typename = "User" };
                    return ValueTask.CompletedTask;
                }}
            },
            ["User"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<dynamic>(u => u.name) },
                { "email", context => context.ResolveAsPropertyOf<dynamic>(u => u.email) }
            },
            ["Post"] = new()
            {
                { "title", context => context.ResolveAsPropertyOf<dynamic>(p => p.title) },
                { "content", context => context.ResolveAsPropertyOf<dynamic>(p => p.content) }
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateErrorThrowingSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                throwingField: String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "throwingField", context => throw new InvalidOperationException("Test resolver exception") }
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateNullReturningSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                nullField: String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "nullField", context =>
                {
                    context.ResolvedValue = null;
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateNonNullFieldSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                nonNullField: String!
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "nonNullField", context =>
                {
                    context.ResolvedValue = null;  // This should cause an error
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateDeprecatedFieldSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                deprecatedField: String @deprecated(reason: ""Use newField instead"")
                newField: String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "deprecatedField", context =>
                {
                    context.ResolvedValue = "This field is deprecated";
                    return ValueTask.CompletedTask;
                }},
                { "newField", context =>
                {
                    context.ResolvedValue = "This is the new field";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateCustomScalarSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            scalar DateTime

            type Query {
                processDateTime(date: DateTime!): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "processDateTime", context =>
                {
                    var dateString = (string)context.ArgumentValues["date"];
                    context.ResolvedValue = $"Processed date: {dateString}";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }
}