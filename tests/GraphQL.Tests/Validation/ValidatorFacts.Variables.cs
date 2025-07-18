using System.Linq;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_58_Variables_valid1()
    {
        /* Given */
        var document =
            @"query A($atOtherHomes: Boolean) {
                      ...HouseTrainedFragment
                    }

                    query B($atOtherHomes: Boolean) {
                      ...HouseTrainedFragment
                    }

                    fragment HouseTrainedFragment on Query {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R581And582Variables());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_58_Variables_invalid1()
    {
        /* Given */
        var document =
            @"query houseTrainedQuery($atOtherHomes: Boolean, $atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R581And582Variables());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R581VariableUniqueness);
    }

    [Fact]
    public void Rule_582_VariablesAreInputTypes_valid1()
    {
        /* Given */
        var document =
            @"query takesBoolean($atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }

                    query takesComplexInput($complexInput: ComplexInput) {
                      findDog(complex: $complexInput) {
                        name
                      }
                    }

                    query TakesListOfBooleanBang($booleans: [Boolean!]) {
                      booleanList(booleanListArg: $booleans)
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R581And582Variables());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_582_VariablesAreInputTypes_invalid1()
    {
        /* Given */
        var document =
            @"query takesCat($cat: Cat) {
                      __typename
                    }

                    query takesDogBang($dog: Dog!) {
                      __typename
                    }

                    query takesListOfPet($pets: [Pet]) {
                      __typename
                    }

                    query takesCatOrDog($catOrDog: CatOrDog) {
                      __typename
                    }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R581And582Variables());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count());
        Assert.Contains(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R582VariablesAreInputTypes
                     && error.Message.StartsWith("Variables can only be input types. Objects, unions,"));
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_valid1()
    {
        /* Given */
        var document =
            @"query variableIsDefined($atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_valid2()
    {
        /* Given */
        var document =
            @"query variableIsDefinedUsedInSingleFragment($atOtherHomes: Boolean) {
                      dog {
                        ...isHousetrainedFragment
                      }
                    }

                    fragment isHousetrainedFragment on Dog {
                      isHousetrained(atOtherHomes: $atOtherHomes)
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_valid3()
    {
        /* Given */
        var document =
            @"query housetrainedQueryOne($atOtherHomes: Boolean) {
                      dog {
                        ...isHousetrainedFragment
                      }
                }

                query housetrainedQueryTwo($atOtherHomes: Boolean) {
                  dog {
                    ...isHousetrainedFragment
                  }
                }

                fragment isHousetrainedFragment on Dog {
                  isHousetrained(atOtherHomes: $atOtherHomes)
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_invalid1()
    {
        /* Given */
        var document =
            @"
            query variableIsNotDefined {
              dog {
                isHousetrained(atOtherHomes: $atOtherHomes)
              }
            }
             ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R583AllVariableUsesDefined);
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_invalid2()
    {
        /* Given */
        var document =
            @"
            query variableIsNotDefinedUsedInSingleFragment {
              dog {
                ...isHousetrainedFragment
              }
            }

            fragment isHousetrainedFragment on Dog {
              isHousetrained(atOtherHomes: $atOtherHomes)
            }
             ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R583AllVariableUsesDefined);
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_invalid3()
    {
        /* Given */
        var document =
            @"
            query variableIsNotDefinedUsedInNestedFragment {
              dog {
                ...outerHousetrainedFragment
              }
            }

            fragment outerHousetrainedFragment on Dog {
              ...isHousetrainedFragment
            }

            fragment isHousetrainedFragment on Dog {
              isHousetrained(atOtherHomes: $atOtherHomes)
            }
             ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R583AllVariableUsesDefined);
    }

    [Fact]
    public void Rule_583_AllVariableUsesDefined_invalid4()
    {
        /* Given */
        var document =
            @"
            query housetrainedQueryOne($atOtherHomes: Boolean) {
              dog {
                ...isHousetrainedFragment
              }
            }

            query housetrainedQueryTwoNotDefined {
              dog {
                ...isHousetrainedFragment
              }
            }

            fragment isHousetrainedFragment on Dog {
              isHousetrained(atOtherHomes: $atOtherHomes)
            }
             ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R583AllVariableUsesDefined());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R583AllVariableUsesDefined);
    }

    [Fact]
    public void Rule_584_AllVariablesUsed_invalid1()
    {
        /* Given */
        var document =
            @"
                query variableUnused($atOtherHomes: Boolean) {
                  dog {
                    isHousetrained
                  }
                }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R584AllVariablesUsed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R584AllVariablesUsed
                     && error.Message.Contains("atOtherHomes"));
    }

    [Fact]
    public void Rule_584_AllVariablesUsed_invalid2()
    {
        /* Given */
        var document =
            @"
                query variableNotUsedWithinFragment($atOtherHomes: Boolean) {
                  dog {
                    ...isHousetrainedWithoutVariableFragment
                  }
                }

                fragment isHousetrainedWithoutVariableFragment on Dog {
                  isHousetrained
                }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R584AllVariablesUsed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R584AllVariablesUsed
                     && error.Message.Contains("atOtherHomes"));
    }

    [Fact]
    public void Rule_584_AllVariablesUsed_invalid3()
    {
        /* Given */
        var document =
            @"
                query queryWithUsedVar($atOtherHomes: Boolean) {
                  dog {
                    ...isHousetrainedFragment
                  }
                }

                query queryWithExtraVar($atOtherHomes: Boolean, $extra: Int) {
                  dog {
                    ...isHousetrainedFragment
                  }
                }

                fragment isHousetrainedFragment on Dog {
                  isHousetrained(atOtherHomes: $atOtherHomes)
                }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R584AllVariablesUsed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R584AllVariablesUsed
                     && error.Message.Contains("extra"));
    }

    [Fact]
    public void Rule_584_AllVariablesUsed_valid1()
    {
        /* Given */
        var document =
            @"
                query variableUsedInFragment($atOtherHomes: Boolean) {
                  dog {
                    ...isHousetrainedFragment
                  }
                }

                fragment isHousetrainedFragment on Dog {
                  isHousetrained(atOtherHomes: $atOtherHomes)
                }
                 ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R584AllVariablesUsed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_invalid1()
    {
        /* Given */
        var document =
            @"
                query intCannotGoIntoBoolean($intArg: Int) {
                  arguments {
                    booleanArgField(booleanArg: $intArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_invalid2()
    {
        /* Given */
        var document =
            @"
                query booleanListCannotGoIntoBoolean($booleanListArg: [Boolean]) {
                  arguments {
                    booleanArgField(booleanArg: $booleanListArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_invalid3()
    {
        /* Given */
        var document =
            @"
                query booleanArgQuery($booleanArg: Boolean) {
                  arguments {
                    nonNullBooleanArgField(nonNullBooleanArg: $booleanArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_valid1()
    {
        /* Given */
        var document =
            @"
                query nonNullListToList($nonNullBooleanList: [Boolean]!) {
                  arguments {
                    booleanListArgField(booleanListArg: $nonNullBooleanList)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_invalid4()
    {
        /* Given */
        var document =
            @"
                query listToNonNullList($booleanList: [Boolean]) {
                  arguments {
                    nonNullBooleanListField(nonNullBooleanListArg: $booleanList)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_valid2()
    {
        /* Given */
        var document =
            @"
                query booleanArgQueryWithDefault($booleanArg: Boolean) {
                  arguments {
                    optionalNonNullBooleanArgField(optionalBooleanArg: $booleanArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_valid3()
    {
        /* Given */
        var document =
            @"
                query booleanArgQueryWithDefault($booleanArg: Boolean = true) {
                  arguments {
                    nonNullBooleanArgField(nonNullBooleanArg: $booleanArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Edge case tests for R5.8.5 - Variable coercion with complex scenarios
    /// </summary>
    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_nullable_to_nonnull_with_default()
    {
        /* Given */
        var document =
            @"
                query nullableToNonNullWithDefault($nullableArg: Boolean) {
                  arguments {
                    optionalNonNullBooleanArgField(optionalBooleanArg: $nullableArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_nested_list_coercion_invalid()
    {
        /* Given */
        var document =
            @"
                query nestedListCoercionInvalid($nestedList: [[Boolean]]) {
                  arguments {
                    booleanListArgField(booleanListArg: $nestedList)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_complex_input_coercion()
    {
        /* Given */
        var document =
            @"
                query complexInputCoercion($complexInput: ComplexInput!) {
                  findDog(complex: $complexInput) {
                    name
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_nullable_list_to_nonnull_list_invalid()
    {
        /* Given */
        var document =
            @"
                query nullableListToNonNullListInvalid($nullableList: [Boolean]) {
                  arguments {
                    nonNullBooleanListField(nonNullBooleanListArg: $nullableList)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_variable_with_default_to_nonnull_valid()
    {
        /* Given */
        var document =
            @"
                query variableWithDefaultToNonNull($booleanArg: Boolean = false) {
                  arguments {
                    nonNullBooleanArgField(nonNullBooleanArg: $booleanArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_int_to_float_coercion_valid()
    {
        /* Given */
        var document =
            @"
                query intToFloatCoercion($intArg: Int!) {
                  arguments {
                    floatArgField(floatArg: $intArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_float_to_int_coercion_invalid()
    {
        /* Given */
        var document =
            @"
                query floatToIntCoercionInvalid($floatArg: Float!) {
                  arguments {
                    intArgField(intArg: $floatArg)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }

    [Fact]
    public void Rule_585_AllVariableUsagesAreAllowed_edge_case_null_variable_with_nonnull_arg_invalid()
    {
        /* Given */
        var document =
            @"
                query nullVariableWithNonNullArg($nullVar: Boolean) {
                  arguments {
                    nonNullBooleanArgField(nonNullBooleanArg: $nullVar)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R585AllVariableUsagesAreAllowed);
    }
}