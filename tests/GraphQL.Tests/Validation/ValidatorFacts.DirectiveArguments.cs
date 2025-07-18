using System.Collections.Generic;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    /// <summary>
    /// Directive argument validation tests
    /// </summary>
    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_required_argument_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @customDirective(requiredArg: String!) on FIELD | QUERY
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query @customDirective(requiredArg: ""value"") {
              field
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_missing_required_argument_invalid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @customDirective(requiredArg: String!) on FIELD | QUERY
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query @customDirective {
              field
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_optional_argument_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @optionalDirective(optionalArg: String = ""default"") on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @optionalDirective
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_multiple_arguments_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @multiArgDirective(
                  requiredArg: String!,
                  optionalArg: Int = 10,
                  anotherRequired: Boolean!
                ) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @multiArgDirective(
                requiredArg: ""value"",
                optionalArg: 20,
                anotherRequired: true
              )
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_missing_one_required_argument_invalid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @multiArgDirective(
                  requiredArg: String!,
                  optionalArg: Int = 10,
                  anotherRequired: Boolean!
                ) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @multiArgDirective(
                requiredArg: ""value""
              )
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
    }

    [Fact]
    public void Rule_541_ArgumentNames_directive_with_unknown_argument_invalid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @testDirective(knownArg: String) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @testDirective(unknownArg: ""value"")
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R541ArgumentNames() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R541ArgumentNames);
    }

    [Fact]
    public void Rule_542_ArgumentUniqueness_directive_with_duplicate_arguments_invalid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @testDirective(arg: String) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @testDirective(arg: ""value1"", arg: ""value2"")
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R542ArgumentUniqueness() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R542ArgumentUniqueness);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_directive_with_wrong_argument_type_invalid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @testDirective(intArg: Int!) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @testDirective(intArg: ""not_an_int"")
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R561ValuesOfCorrectType() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R561ValuesOfCorrectType);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_variable_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @variableDirective(requiredArg: String!) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query testWithVariable($arg: String!) {
              field @variableDirective(requiredArg: $arg)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document),
            new Dictionary<string, object> { ["arg"] = "value" });

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_null_variable_invalid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @variableDirective(requiredArg: String!) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query testWithNullVariable($arg: String) {
              field @variableDirective(requiredArg: $arg)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document),
            new Dictionary<string, object> { ["arg"] = null });

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_input_object_argument_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                input DirectiveInput {
                  name: String!
                  value: Int
                }
                
                directive @complexDirective(input: DirectiveInput!) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @complexDirective(input: { name: ""test"", value: 42 })
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_list_argument_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                directive @listDirective(items: [String!]!) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @listDirective(items: [""item1"", ""item2"", ""item3""])
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_RequiredArguments_directive_with_enum_argument_valid()
    {
        /* Given */
        var directiveSchema = new SchemaBuilder()
            .Add(@"
                enum DirectiveEnum {
                  OPTION_A
                  OPTION_B
                  OPTION_C
                }
                
                directive @enumDirective(option: DirectiveEnum!) on FIELD
                
                type Query {
                  field: String
                }
            ")
            .Build(new SchemaBuildOptions()).Result;

        var document = @"
            query {
              field @enumDirective(option: OPTION_A)
            }
            ";

        /* When */
        var result = Validator.Validate(
            new[] { ExecutionRules.R5421RequiredArguments() },
            directiveSchema,
            Parser.ParseExecutableDocument(document));

        /* Then */
        Assert.True(result.IsValid);
    }
}