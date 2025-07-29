using Tanka.GraphQL.Validation;

using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_541_Argument_Names_valid1()
    {
        /* Given */
        var document =
            @"fragment argOnRequiredArg on Dog {
                      doesKnowCommand(dogCommand: SIT)
                    }

                    fragment argOnOptional on Dog {
                      isHousetrained(atOtherHomes: true) @include(if: true)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R541ArgumentNames());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_541_Argument_Names_invalid1()
    {
        /* Given */
        var document =
            @"fragment invalidArgName on Dog {
                      doesKnowCommand(command: CLEAN_UP_HOUSE)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R541ArgumentNames());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R541ArgumentNames);
    }

    [Fact]
    public void Rule_541_Argument_Names_invalid2()
    {
        /* Given */
        var document =
            @"fragment invalidArgName on Dog {
                      isHousetrained(atOtherHomes: true) @include(unless: false)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R541ArgumentNames());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R541ArgumentNames);
    }

    [Fact]
    public void Rule_542_Argument_Uniqueness_valid1()
    {
        /* Given */
        var document =
            @"fragment argOnRequiredArg on Dog {
                      doesKnowCommand(dogCommand: SIT)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R542ArgumentUniqueness());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_542_Argument_Uniqueness_invalid1()
    {
        /* Given */
        var document =
            @"fragment invalidArgName on Dog {
                      doesKnowCommand(command: SIT, command: SIT)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R542ArgumentUniqueness());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R542ArgumentUniqueness);
    }

    [Fact]
    public void Rule_542_Argument_Uniqueness_invalid2()
    {
        /* Given */
        var document =
            @"fragment invalidArgName on Dog {
                      doesKnowCommand(command: SIT) @skip(if: true, if: true)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R542ArgumentUniqueness());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R542ArgumentUniqueness);
    }

    [Fact]
    public void Rule_5421_Required_Arguments_valid1()
    {
        /* Given */
        var document =
            @"fragment goodBooleanArg on Arguments {
                      booleanArgField(booleanArg: true)
                    }

                    fragment goodNonNullArg on Arguments {
                      nonNullBooleanArgField(nonNullBooleanArg: true)
                    }

                    fragment goodBooleanArgDefault on Arguments {
                      booleanArgField
                    }
                    ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5421RequiredArguments());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5421_Required_Arguments_invalid1()
    {
        /* Given */
        var document =
            @"fragment missingRequiredArg on Arguments {
                      nonNullBooleanArgField
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5421RequiredArguments());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
    }

    [Fact]
    public void Rule_5421_Required_Arguments_invalid2()
    {
        /* Given */
        var document =
            @"fragment missingRequiredArg on Arguments {
                      nonNullBooleanArgField(nonNullBooleanArg: null)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5421RequiredArguments());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5421RequiredArguments);
    }
}