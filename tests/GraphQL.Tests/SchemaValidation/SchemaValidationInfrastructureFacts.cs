using System;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.SchemaValidation;

using Xunit;

namespace Tanka.GraphQL.Tests.SchemaValidation;

public class SchemaValidationInfrastructureFacts
{
    [Fact]
    public async Task Schema_validation_should_fail_for_oneOf_with_default_value()
    {
        // Given
        var builder = new SchemaBuilder();
        builder.Add(@"
            input TestInput @oneOf {
                field1: String = ""default""
                field2: Int
            }
            
            type Query {
                test(input: TestInput): String
            }
        ");

        // When & Then
        var exception = await Assert.ThrowsAsync<SchemaValidationException>(async () =>
        {
            await builder.Build();
        });

        // Debug: Check the actual error message and error details
        Assert.NotEmpty(exception.Errors);
        var firstError = exception.Errors.First();
        Assert.Contains("cannot have a default value", firstError.Message);
        Assert.Single(exception.Errors);
    }

    [Fact]
    public async Task Schema_validation_should_pass_for_valid_oneOf()
    {
        // Given
        var builder = new SchemaBuilder();
        builder.Add(@"
            input ValidOneOfInput @oneOf {
                field1: String
                field2: Int
            }
            
            type Query {
                test(input: ValidOneOfInput): String
            }
        ");

        // When
        var schema = await builder.Build();

        // Then
        Assert.NotNull(schema);
        var inputType = schema.GetNamedType("ValidOneOfInput");
        Assert.NotNull(inputType);
    }

    [Fact]
    public async Task Schema_validation_should_fail_for_oneOf_with_no_fields()
    {
        // Given
        var builder = new SchemaBuilder();
        builder.Add(@"
            input EmptyOneOfInput @oneOf {
            }
            
            type Query {
                test(input: EmptyOneOfInput): String
            }
        ");

        // When & Then
        var exception = await Assert.ThrowsAsync<SchemaValidationException>(async () =>
        {
            await builder.Build();
        });

        Assert.Single(exception.Errors);
        Assert.Contains("must have at least one field", exception.Errors.First().Message);
    }
}