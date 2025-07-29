using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.ValueSerialization;

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
        // Two validation errors are expected: one for required field missing, one for field not provided
        Assert.Equal(2, result.Errors.Count);
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
        Assert.Null(result.Errors);

        var animals = (List<object>)result.Data["animals"];
        Assert.Equal(2, animals.Count);

        var dog = (Dictionary<string, object?>)animals[0];
        Assert.Equal("Buddy", dog["name"]);
        Assert.Equal("Woof", dog["sound"]);
        Assert.Equal("Golden Retriever", dog["breed"]);

        var cat = (Dictionary<string, object?>)animals[1];
        Assert.Equal("Whiskers", cat["name"]);
        Assert.Equal("Meow", cat["sound"]);
        Assert.Equal("Orange", cat["color"]);
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
        Assert.Null(result.Errors);

        var searchResult = (Dictionary<string, object?>)result.Data["searchResult"];
        Assert.Equal("John Doe", searchResult["name"]);
        Assert.Equal("john@example.com", searchResult["email"]);
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
        Assert.Contains("Cannot complete value for field", result.Errors[0].Message);
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
        Assert.Equal("Processed date: 2023-01-01T00:00:00Z", result.Data["processDateTime"]);
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
                    var name = context.ArgumentValues.TryGetValue("name", out var nameValue) && nameValue != null
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
                    var email = input.TryGetValue("email", out var emailValue) && emailValue != null
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
                    var sum = numbers.Cast<int>().Sum();
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
                    if (!context.ArgumentValues.TryGetValue("status", out var statusValue) || statusValue == null)
                    {
                        context.ResolvedValue = "Status is unknown";
                        return ValueTask.CompletedTask;
                    }
                    
                    // Enum values come through as EnumValue objects
                    string status;
                    if (statusValue is EnumValue enumValue)
                    {
                        status = enumValue.Name.Value;
                    }
                    else
                    {
                        status = statusValue.ToString() ?? "unknown";
                    }
                    
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
                        new TestNamedType("Dog") { name = "Buddy", sound = "Woof", breed = "Golden Retriever" },
                        new TestNamedType("Cat") { name = "Whiskers", sound = "Meow", color = "Orange" }
                    };
                    return ValueTask.CompletedTask;
                }}
            },
            ["Dog"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<TestNamedType>(d => d.name) },
                { "sound", context => context.ResolveAsPropertyOf<TestNamedType>(d => d.sound) },
                { "breed", context => context.ResolveAsPropertyOf<TestNamedType>(d => d.breed) }
            },
            ["Cat"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<TestNamedType>(c => c.name) },
                { "sound", context => context.ResolveAsPropertyOf<TestNamedType>(c => c.sound) },
                { "color", context => context.ResolveAsPropertyOf<TestNamedType>(c => c.color) }
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
                    context.ResolvedValue = new TestNamedType("User") { name = "John Doe", email = "john@example.com" };
                    return ValueTask.CompletedTask;
                }}
            },
            ["User"] = new()
            {
                { "name", context => context.ResolveAsPropertyOf<TestNamedType>(u => u.name) },
                { "email", context => context.ResolveAsPropertyOf<TestNamedType>(u => u.email) }
            },
            ["Post"] = new()
            {
                { "title", context => context.ResolveAsPropertyOf<TestNamedType>(p => p.title) },
                { "content", context => context.ResolveAsPropertyOf<TestNamedType>(p => p.content) }
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
                    var dateValue = context.ArgumentValues["date"];
                    string dateString;
                    
                    if (dateValue is DateTime dateTime)
                    {
                        dateString = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        dateString = dateValue?.ToString() ?? "null";
                    }
                    
                    context.ResolvedValue = $"Processed date: {dateString}";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        // Register DateTime converter with default converters
        var valueConverters = new Dictionary<string, IValueConverter>
        {
            ["String"] = new StringConverter(),
            ["Int"] = new IntConverter(),
            ["Float"] = new DoubleConverter(),
            ["Boolean"] = new BooleanConverter(),
            ["ID"] = new IdConverter(),
            ["DateTime"] = new DateTimeConverter()
        };
        
        var buildOptions = new SchemaBuildOptions
        {
            Resolvers = resolvers,
            ValueConverters = valueConverters
        };

        return await builder.Build(buildOptions);
    }

    private class DateTimeConverter : IValueConverter
    {
        public object? Serialize(object? value)
        {
            if (value == null)
                return null;

            if (value is DateTime dateTime)
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            return value.ToString();
        }

        public ValueBase SerializeLiteral(object? value)
        {
            var serializedValue = Serialize(value);
            if (serializedValue == null)
                return new NullValue();

            return new StringValue(Encoding.UTF8.GetBytes((string)serializedValue));
        }

        public object? ParseValue(object? input)
        {
            if (input == null)
                return null;

            if (input is string stringValue)
            {
                if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTime))
                    return dateTime;
            }

            return input;
        }

        public object? ParseLiteral(ValueBase input)
        {
            if (input.Kind == NodeKind.NullValue) 
                return null;

            if (input.Kind == NodeKind.StringValue)
            {
                var stringValue = ((StringValue)input).ToString();
                if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTime))
                    return dateTime;
                
                return stringValue; // Return original string if parsing fails
            }

            throw new FormatException($"Cannot coerce DateTime value from '{input.Kind}'");
        }
    }

    private class TestNamedType : DynamicObject, INamedType
    {
        public string __Typename { get; }
        private readonly Dictionary<string, object?> _properties = new();

        public TestNamedType(string typename)
        {
            __Typename = typename;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            _properties[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            return _properties.TryGetValue(binder.Name, out result);
        }

        public dynamic name { get => _properties.GetValueOrDefault("name"); set => _properties["name"] = value; }
        public dynamic sound { get => _properties.GetValueOrDefault("sound"); set => _properties["sound"] = value; }
        public dynamic breed { get => _properties.GetValueOrDefault("breed"); set => _properties["breed"] = value; }
        public dynamic color { get => _properties.GetValueOrDefault("color"); set => _properties["color"] = value; }
        public dynamic email { get => _properties.GetValueOrDefault("email"); set => _properties["email"] = value; }
        public dynamic title { get => _properties.GetValueOrDefault("title"); set => _properties["title"] = value; }
        public dynamic content { get => _properties.GetValueOrDefault("content"); set => _properties["content"] = value; }
    }
}