using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Extensions.Experimental.Tests.OneOf;

public class ValidationRuleFacts
{
    [Fact]
    public async Task Valid_when_one_field_set()
    {
        /* Given */
        ISchema schema = await new SchemaBuilder()
            .Add("""
                 input OneOfInput @oneOf {
                    a: String
                    b: String
                 }

                 type Query {
                    oneOf(input: OneOfInput!): String
                 }
                 """)
            .Build(new SchemaBuildOptions());

        var validator = new AsyncValidator(ExecutionRules.All);

        /* When */
        ValidationResult result = await validator.Validate(schema, """
                                                                   {
                                                                      oneOf(input: { a: "a" })
                                                                   }
                                                                   """,
            new Dictionary<string, object?>()
        );

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Valid_when_one_field_set_for_nullable()
    {
        /* Given */
        ISchema schema = await new SchemaBuilder()
            .Add("""
                 input OneOfInput @oneOf {
                    a: String
                    b: String
                 }

                 type Query {
                    oneOf(input: OneOfInput): String
                 }
                 """)
            .Build(new SchemaBuildOptions());

        var validator = new AsyncValidator(ExecutionRules.All);

        /* When */
        ValidationResult result = await validator.Validate(schema, """
                                                                   {
                                                                      oneOf(input: { a: "a" })
                                                                   }
                                                                   """,
            new Dictionary<string, object?>()
        );

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Invalid_when_two_field_set()
    {
        /* Given */
        ISchema schema = await new SchemaBuilder()
            .Add("""
                 input OneOfInput @oneOf {
                    a: String
                    b: String
                 }

                 type Query {
                    oneOf(input: OneOfInput!): String
                 }
                 """)
            .Build(new SchemaBuildOptions());

        var validator = new AsyncValidator(ExecutionRules.All);

        /* When */
        ValidationResult result = await validator.Validate(schema, """
                                                                   {
                                                                      oneOf(input: { a: "a", b: "b" })
                                                                   }
                                                                   """,
            new Dictionary<string, object?>()
        );

        /* Then */
        Assert.False(result.IsValid);
        Assert.Equal("ONEOF001", result.Errors.Single().Code);
    }

    [Fact]
    public async Task Invalid_when_two_field_set_for_nullable()
    {
        /* Given */
        ISchema schema = await new SchemaBuilder()
            .Add("""
                 input OneOfInput @oneOf {
                    a: String
                    b: String
                 }

                 type Query {
                    oneOf(input: OneOfInput): String
                 }
                 """)
            .Build(new SchemaBuildOptions());

        var validator = new AsyncValidator(ExecutionRules.All);

        /* When */
        ValidationResult result = await validator.Validate(schema, """
                                                                   {
                                                                      oneOf(input: { a: "a", b: "b" })
                                                                   }
                                                                   """,
            new Dictionary<string, object?>()
        );

        /* Then */
        Assert.False(result.IsValid);
        Assert.Equal("ONEOF001", result.Errors.Single().Code);
    }

    [Fact]
    public async Task Valid_when_one_field_set_as_variable()
    {
        /* Given */
        ISchema schema = await new SchemaBuilder()
            .Add("""
                 input OneOfInput @oneOf {
                    a: String
                    b: String
                 }

                 type Query {
                    oneOf(input: OneOfInput!): String
                 }
                 """)
            .Build(new SchemaBuildOptions());

        var validator = new AsyncValidator(ExecutionRules.All);

        /* When */
        ValidationResult result = await validator.Validate(schema, """
                                                                   query ($variable: OneOfInput!) {
                                                                      oneOf(input: $variable)
                                                                   }
                                                                   """,
            new Dictionary<string, object?> { ["variable"] = new Dictionary<string, object?> { ["a"] = "a" } }
        );

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Invalid_when_two_field_set_as_variable()
    {
        /* Given */
        ISchema schema = await new SchemaBuilder()
            .Add("""
                 input OneOfInput @oneOf {
                    a: String
                    b: String
                 }

                 type Query {
                    oneOf(input: OneOfInput!): String
                 }
                 """)
            .Build(new SchemaBuildOptions());

        var validator = new AsyncValidator(ExecutionRules.All);

        /* When */
        ValidationResult result = await validator.Validate(schema, """
                                                                   query ($variable: OneOfInput!) {
                                                                      oneOf(input: $variable)
                                                                   }
                                                                   """,
            new Dictionary<string, object?>
            {
                ["variable"] = new Dictionary<string, object?> { ["a"] = "a", ["b"] = "b" }
            }
        );

        /* Then */
        Assert.False(result.IsValid);
        Assert.Equal("ONEOF001", result.Errors.Single().Code);
    }
}