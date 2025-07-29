using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class ExecutionEngineRobustnessFacts
{
    [Fact]
    public async Task Execute_WithNullQuery_ShouldHandleGracefully()
    {
        var schema = await CreateTestSchema();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Executor.Execute(schema, null!));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task Execute_WithEmptyQuery_ShouldHandleGracefully()
    {
        var schema = await CreateTestSchema();

        var result = await Executor.Execute(schema, "");

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithWhitespaceOnlyQuery_ShouldHandleGracefully()
    {
        var schema = await CreateTestSchema();

        var result = await Executor.Execute(schema, "   \t\n  ");

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithInvalidSyntaxQuery_ShouldReturnSyntaxError()
    {
        var schema = await CreateTestSchema();

        var result = await Executor.Execute(schema, "{ field_with_missing_brace");

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Unexpected end of input", result.Errors[0].Message);
    }

    [Fact]
    public async Task Execute_WithNonExistentField_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var result = await Executor.Execute(schema, "{ nonExistentField }");

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithDeeplyNestedQuery_ShouldHandleGracefully()
    {
        var schema = await CreateNestedTestSchema();

        // Create deeply nested query
        var query = "{ root";
        for (int i = 0; i < 100; i++)
        {
            query += " { nested";
        }
        query += " { value }";
        for (int i = 0; i < 100; i++)
        {
            query += " }";
        }
        query += " }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        // Should either succeed or fail gracefully without crashing
    }

    [Fact]
    public async Task Execute_WithCircularFragments_ShouldDetectAndPreventInfiniteLoop()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query {
                hello
                ...FragA
            }
            
            fragment FragA on Query {
                hello
                ...FragB
            }
            
            fragment FragB on Query {
                hello
                ...FragA
            }
        ";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithVeryLargeQueryDocument_ShouldHandlePerformance()
    {
        var schema = await CreateTestSchema();

        // Generate large query with many fields
        var query = "{ ";
        for (int i = 0; i < 1000; i++)
        {
            query += $"alias{i}: hello ";
        }
        query += "}";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        // Should complete within reasonable time and memory constraints
    }

    [Fact]
    public async Task Execute_WithResolverThatThrowsException_ShouldCaptureError()
    {
        var schema = await CreateSchemaWithThrowingResolver();

        var result = await Executor.Execute(schema, "{ throwingField }");

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Test exception", result.Errors[0].Message);
    }

    [Fact]
    public async Task Execute_WithResolverThatReturnsNull_ShouldHandleGracefully()
    {
        var schema = await CreateSchemaWithNullResolver();

        var result = await Executor.Execute(schema, "{ nullField }");

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data["nullField"]);
    }

    [Fact]
    public async Task Execute_WithInvalidVariableValues_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($name: String!) {
                greeting(name: $name)
            }
        ";

        var variables = new Dictionary<string, object?>
        {
            ["name"] = null // null for non-null variable
        };

        var result = await Executor.Execute(schema, query, variables);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithMissingRequiredVariable_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query($name: String!) {
                greeting(name: $name)
            }
        ";

        var result = await Executor.Execute(schema, query, null);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithInvalidArgumentType_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ greeting(name: 123) }"; // number instead of string

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithUnknownDirective_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"{ hello @unknownDirective }";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithInvalidFragmentType_ShouldReturnValidationError()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query {
                hello
                ...InvalidFragment
            }
            
            fragment InvalidFragment on NonExistentType {
                someField
            }
        ";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithResolverTimeout_ShouldHandleGracefully()
    {
        var schema = await CreateSchemaWithSlowResolver();

        var result = await Executor.Execute(schema, "{ slowField }");

        Assert.NotNull(result);
        // Should either complete or timeout gracefully
    }

    [Fact]
    public async Task Execute_WithMultipleOperations_ShouldRequireOperationName()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query Operation1 {
                hello
            }
            
            query Operation2 {
                greeting(name: ""test"")
            }
        ";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithSpecificOperationName_ShouldExecuteCorrectOperation()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query Operation1 {
                hello
            }
            
            query Operation2 {
                greeting(name: ""test"")
            }
        ";

        var result = await Executor.Execute(schema, query, operationName: "Operation2");

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("Hello, test!", result.Data["greeting"]);
    }

    [Fact]
    public async Task Execute_WithNonExistentOperationName_ShouldReturnError()
    {
        var schema = await CreateTestSchema();

        var query = @"
            query Operation1 {
                hello
            }
        ";

        var result = await Executor.Execute(schema, query, operationName: "NonExistentOperation");

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Execute_WithMutationAsQuery_ShouldReturnValidationError()
    {
        var schema = await CreateSchemaWithMutation();

        var query = @"
            query {
                createUser(name: ""test"")
            }
        ";

        var result = await Executor.Execute(schema, query);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    private async Task<ISchema> CreateTestSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                hello: String
                greeting(name: String!): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "hello", context =>
                {
                    context.ResolvedValue = "Hello, World!";
                    return ValueTask.CompletedTask;
                }},
                { "greeting", context =>
                {
                    var name = (string)context.ArgumentValues["name"];
                    context.ResolvedValue = $"Hello, {name}!";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateNestedTestSchema()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                root: NestedType
            }
            
            type NestedType {
                nested: NestedType
                value: String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "root", context =>
                {
                    context.ResolvedValue = new object();
                    return ValueTask.CompletedTask;
                }}
            },
            ["NestedType"] = new()
            {
                { "nested", context =>
                {
                    context.ResolvedValue = new object();
                    return ValueTask.CompletedTask;
                }},
                { "value", context =>
                {
                    context.ResolvedValue = "leaf value";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateSchemaWithThrowingResolver()
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
                { "throwingField", context => throw new InvalidOperationException("Test exception") }
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateSchemaWithNullResolver()
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

    private async Task<ISchema> CreateSchemaWithSlowResolver()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                slowField: String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "slowField", async context =>
                {
                    await Task.Delay(100); // Simulate slow operation
                    context.ResolvedValue = "Slow result";
                }}
            }
        };

        return await builder.Build(resolvers);
    }

    private async Task<ISchema> CreateSchemaWithMutation()
    {
        var builder = new SchemaBuilder();
        builder.Add(@"
            type Query {
                hello: String
            }
            
            type Mutation {
                createUser(name: String!): String
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "hello", context =>
                {
                    context.ResolvedValue = "Hello, World!";
                    return ValueTask.CompletedTask;
                }}
            },
            ["Mutation"] = new()
            {
                { "createUser", context =>
                {
                    var name = (string)context.ArgumentValues["name"];
                    context.ResolvedValue = $"Created user: {name}";
                    return ValueTask.CompletedTask;
                }}
            }
        };

        return await builder.Build(resolvers);
    }
}