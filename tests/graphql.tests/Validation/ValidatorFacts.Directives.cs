using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_57_DirectivesAreDefined_valid1()
    {
        /* Given */
        var document =
            @"{
                       findDog(complex: { name: ""Fido"" }) @skip(if: false)
                  }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R571And573Directives());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_571_DirectivesAreDefined_invalid1()
    {
        /* Given */
        var document = 
            @"{
                       findDog(complex: { name: ""Fido"" }) @doesNotExists
                  }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R571And573Directives());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R571DirectivesAreDefined);
    }

    [Fact]
    public void Rule_572_DirectivesAreInValidLocations_valid1()
    {
        /* Given */
        var document = 
            @"
                query {
                  field @skip(if: $foo)
                }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R572DirectivesAreInValidLocations());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_572_DirectivesAreInValidLocations_invalid1()
    {
        /* Given */
        var document = 
            @"
                query @skip(if: $foo) {
                  field
                }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R572DirectivesAreInValidLocations());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R572DirectivesAreInValidLocations);
    }

    [Fact]
    public void Rule_573_DirectivesAreUniquePerLocation_valid1()
    {
        /* Given */
        var document =
            @"query ($foo: Boolean = true, $bar: Boolean = false) {
                      field @skip(if: $foo) {
                        subfieldA
                      }
                      field @skip(if: $bar) {
                        subfieldB
                      }
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R571And573Directives());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_573_DirectivesAreUniquePerLocation_invalid1()
    {
        /* Given */
        var document = 
            @"query ($foo: Boolean = true, $bar: Boolean = false) {
                      field @skip(if: $foo) @skip(if: $bar)
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R571And573Directives());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R573DirectivesAreUniquePerLocation);
    }
}