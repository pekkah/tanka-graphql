using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_5511_Fragment_Name_Uniqueness_valid1()
    {
        /* Given */
        var document = @"{
                      dog {
                        ...fragmentOne
                        ...fragmentTwo
                      }
                    }

                    fragment fragmentOne on Dog {
                      name
                    }

                    fragment fragmentTwo on Dog {
                      owner {
                        name
                      }
                    }
                  ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5511FragmentNameUniqueness());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5511_Fragment_Name_Uniqueness_invalid1()
    {
        /* Given */
        var document =
            @"{
                      dog {
                        ...fragmentOne
                      }
                    }

                    fragment fragmentOne on Dog {
                      name
                    }

                    fragment fragmentOne on Dog {
                      owner {
                        name
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5511FragmentNameUniqueness());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5511FragmentNameUniqueness);
    }

    [Fact]
    public void Rule_5512_Fragment_Spread_Type_Existence_valid1()
    {
        /* Given */
        var document =
            @"fragment correctType on Dog {
                        name
                    }

                    fragment inlineFragment on Dog {
                      ... on Dog {
                        name
                      }
                    }

                    fragment inlineFragment2 on Dog {
                      ... @include(if: true) {
                        name
                      }
                    }
                  ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5512FragmentSpreadTypeExistence());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5512_Fragment_Spread_Type_Existence_invalid1()
    {
        /* Given */
        var document =
            @"fragment notOnExistingType on NotInSchema {
                      name
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5512FragmentSpreadTypeExistence());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5512FragmentSpreadTypeExistence);
    }

    [Fact]
    public void Rule_5512_Fragment_Spread_Type_Existence_invalid2()
    {
        /* Given */
        var document =
            @"fragment inlineNotExistingType on Dog {
                      ... on NotInSchema {
                        name
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5512FragmentSpreadTypeExistence());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5512FragmentSpreadTypeExistence);
    }

    [Fact]
    public void Rule_5513_FragmentsOnCompositeTypes_valid1()
    {
        /* Given */
        var document =
            @"fragment fragOnObject on Dog {
                      name
                    }

                    fragment fragOnInterface on Pet {
                      name
                    }

                    fragment fragOnUnion on CatOrDog {
                      ... on Dog {
                        name
                      }
                    }
                  ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5513FragmentsOnCompositeTypes());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5513_FragmentsOnCompositeTypes_invalid1()
    {
        /* Given */
        var document =
            @"fragment fragOnScalar on Int {
                      something
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5513FragmentsOnCompositeTypes());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5513FragmentsOnCompositeTypes);
    }

    [Fact]
    public void Rule_5513_FragmentsOnCompositeTypes_invalid2()
    {
        /* Given */
        var document =
            @"fragment inlineFragOnScalar on Dog {
                      ... on Boolean {
                        somethingElse
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5513FragmentsOnCompositeTypes());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5513FragmentsOnCompositeTypes);
    }

    [Fact]
    public void Rule_5514_FragmentsMustBeUsed_valid1()
    {
        /* Given */
        var document =
            @"fragment nameFragment on Dog {
                      name
                    }

                    {
                      dog {
                        ...nameFragment
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5514FragmentsMustBeUsed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5514_FragmentsMustBeUsed_invalid1()
    {
        /* Given */
        var document =
            @"fragment nameFragment on Dog {
                      name
                    }

                    {
                      dog {
                        name
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5514FragmentsMustBeUsed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5514FragmentsMustBeUsed);
    }

    [Fact]
    public void Rule_5521_FragmentSpreadTargetDefined_invalid1()
    {
        /* Given */
        var document =
            @"
                {
                  dog {
                    ...undefinedFragment
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5521FragmentSpreadTargetDefined());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5521FragmentSpreadTargetDefined);
    }

    [Fact]
    public void Rule_5521_FragmentSpreadTargetDefined_valid1()
    {
        /* Given */
        var document =
            @"
                {
                    dog   {
                    ...nameFragment
                    }
                }

                fragment nameFragment on Dog {
                  name
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5521FragmentSpreadTargetDefined());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5522_FragmentSpreadsMustNotFormCycles_invalid1()
    {
        /* Given */
        var document =
            @"{
                      dog {
                        ...nameFragment
                      }
                    }

                    fragment nameFragment on Dog {
                      name
                      ...barkVolumeFragment
                    }

                    fragment barkVolumeFragment on Dog {
                      barkVolume
                      ...nameFragment
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5522FragmentSpreadsMustNotFormCycles());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5522FragmentSpreadsMustNotFormCycles);
    }

    [Fact]
    public void Rule_5522_FragmentSpreadsMustNotFormCycles_invalid2()
    {
        /* Given */
        var document =
            @"{
                      dog {
                        ...dogFragment
                      }
                    }

                    fragment dogFragment on Dog {
                      name
                      owner {
                        ...ownerFragment
                      }
                    }

                    fragment ownerFragment on Dog {
                      name
                      pets {
                        ...dogFragment
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5522FragmentSpreadsMustNotFormCycles());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5522FragmentSpreadsMustNotFormCycles);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_in_scope_valid()
    {
        /* Given */
        var document =
            @"fragment dogFragment on Dog {
                      ... on Dog {
                        barkVolume
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_in_scope_invalid()
    {
        /* Given */
        var document =
            @"fragment catInDogFragmentInvalid on Dog {
                  ... on Cat {
                    meowVolume
                  }
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5523FragmentSpreadIsPossible);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_in_abstract_scope_valid1()
    {
        /* Given */
        var document =
            @"fragment petNameFragment on Pet {
                      name
                    }

                    fragment interfaceWithinObjectFragment on Dog {
                      ...petNameFragment
                    }
                    ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_in_abstract_scope_valid2()
    {
        /* Given */
        var document =
            @"fragment catOrDogNameFragment on CatOrDog {
                      ... on Cat {
                        meowVolume
                      }
                    }

                    fragment unionWithObjectFragment on Dog {
                      ...catOrDogNameFragment
                    }
                    ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_abstract_in_abstract_scope_valid1()
    {
        /* Given */
        var document =
            @"fragment unionWithInterface on Pet {
                      ...dogOrHumanFragment
                    }

                    fragment dogOrHumanFragment on DogOrHuman {
                      ... on Dog {
                        barkVolume
                      }
                    }
                    ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_abstract_in_abstract_scope_invalid()
    {
        /* Given */
        var document =
            @"
            fragment nonIntersectingInterfaces on Pet {
                ...sentientFragment
            }

            fragment sentientFragment on Sentient {
                name
            }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R5523FragmentSpreadIsPossible);
    }

    [Fact]
    public void Rule_5523_FragmentSpreadIsPossible_Interface_in_interface_scope_valid()
    {
        /* Given */
        var document =
            @"
            fragment interfaceWithInterface on Node {
              ...resourceFragment
            }

            fragment resourceFragment on Resource {
              url
            }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R5523FragmentSpreadIsPossible());

        /* Then */
        Assert.True(result.IsValid);
    }
}