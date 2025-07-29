using Tanka.GraphQL.Validation;

using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_561_ValuesOfCorrectType_valid1()
    {
        /* Given */
        var document =
            @"fragment goodBooleanArg on Arguments {
                      booleanArgField(booleanArg: true)
                    }

                    fragment coercedIntIntoFloatArg on Arguments {
                      # Note: The input coercion rules for Float allow Int literals.
                      floatArgField(floatArg: 123)
                    }

                    query goodComplexDefaultValue($search: ComplexInput = { name: ""Fido"" }) {
                      findDog(complex: $search)
                    }
                  ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R561ValuesOfCorrectType());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_invalid1()
    {
        /* Given */
        var document =
            @"fragment stringIntoInt on Arguments {
                      intArgField(intArg: ""123"")
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R561ValuesOfCorrectType());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R561ValuesOfCorrectType);
    }

    [Fact]
    public void Rule_561_ValuesOfCorrectType_invalid2()
    {
        /* Given */
        var document =
            @"query badComplexValue {
                      findDog(complex: { name: 123 })
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R561ValuesOfCorrectType());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R561ValuesOfCorrectType);
    }

    [Fact]
    public void Rule_562_InputObjectFieldNames_valid1()
    {
        /* Given */
        var document =
            @"{
                      findDog(complex: { name: ""Fido"" })
                    }
                  ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R562InputObjectFieldNames());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_562_InputObjectFieldNames_invalid1()
    {
        /* Given */
        var document =
            @"{
                      findDog(complex: { favoriteCookieFlavor: ""Bacon"" })
                    }
                    ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R562InputObjectFieldNames());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R562InputObjectFieldNames);
    }

    [Fact]
    public void Rule_563_InputObjectFieldUniqueness_invalid1()
    {
        /* Given */
        var document =
            @"{
                      field(arg: { field: true, field: false })
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R563InputObjectFieldUniqueness());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R563InputObjectFieldUniqueness);
    }

    [Fact]
    public void Rule_564_InputObjectRequiredFields_invalid1()
    {
        /* Given */
        var document =
            @"{
                       findDog(complex: { owner: ""Fido"" })
                  }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R564InputObjectRequiredFields());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R564InputObjectRequiredFields);
    }

    [Fact]
    public void Rule_564_InputObjectRequiredFields_invalid2()
    {
        /* Given */
        var document =
            @"{
                       findDog(complex: { name: null })
                  }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R564InputObjectRequiredFields());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R564InputObjectRequiredFields);
    }
}