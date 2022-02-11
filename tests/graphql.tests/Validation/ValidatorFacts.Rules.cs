using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_561_ValuesOfCorrectType_valid1()
    {
        /* Given */
        var document = Parser.ParseDocument(
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
                  ");

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
        var document = Parser.ParseDocument(
            @"fragment stringIntoInt on Arguments {
                      intArgField(intArg: ""123"")
                    }"
        );

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
        var document = Parser.ParseDocument(
            @"query badComplexValue {
                      findDog(complex: { name: 123 })
                    }"
        );

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
        var document = Parser.ParseDocument(
            @"{
                      findDog(complex: { name: ""Fido"" })
                    }
                  ");

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
        var document = Parser.ParseDocument(
            @"{
                      findDog(complex: { favoriteCookieFlavor: ""Bacon"" })
                    }
                    ");

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
        var document = Parser.ParseDocument(
            @"{
                      field(arg: { field: true, field: false })
                    }
                 ");

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
        var document = Parser.ParseDocument(
            @"{
                       findDog(complex: { owner: ""Fido"" })
                  }
                 ");

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
        var document = Parser.ParseDocument(
            @"{
                       findDog(complex: { name: null })
                  }
                 ");

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
    public void Rule_57_DirectivesAreDefined_valid1()
    {
        /* Given */
        var document = Parser.ParseDocument(
            @"{
                       findDog(complex: { name: ""Fido"" }) @skip(if: false)
                  }
                 ");

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
        var document = Parser.ParseDocument(
            @"{
                       findDog(complex: { name: ""Fido"" }) @doesNotExists
                  }
                 ");

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
        var document = Parser.ParseDocument(
            @"
                query {
                  field @skip(if: $foo)
                }
                 ");

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
        var document = Parser.ParseDocument(
            @"
                query @skip(if: $foo) {
                  field
                }
                 ");

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
        var document = Parser.ParseDocument(
            @"query ($foo: Boolean = true, $bar: Boolean = false) {
                      field @skip(if: $foo) {
                        subfieldA
                      }
                      field @skip(if: $bar) {
                        subfieldB
                      }
                    }
                 ");

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
        var document = Parser.ParseDocument(
            @"query ($foo: Boolean = true, $bar: Boolean = false) {
                      field @skip(if: $foo) @skip(if: $bar)
                    }
                 ");

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

    [Fact]
    public void Rule_58_Variables_valid1()
    {
        /* Given */
        var document = Parser.ParseDocument(
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
                 ");

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
        var document = Parser.ParseDocument(
            @"query houseTrainedQuery($atOtherHomes: Boolean, $atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }
                 ");

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
        var document = Parser.ParseDocument(
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
                 ");

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
        var document = Parser.ParseDocument(
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
                 ");

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
        var document = Parser.ParseDocument(
            @"query variableIsDefined($atOtherHomes: Boolean) {
                      dog {
                        isHousetrained(atOtherHomes: $atOtherHomes)
                      }
                    }");

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
        var document = Parser.ParseDocument(
            @"query variableIsDefinedUsedInSingleFragment($atOtherHomes: Boolean) {
                      dog {
                        ...isHousetrainedFragment
                      }
                    }

                    fragment isHousetrainedFragment on Dog {
                      isHousetrained(atOtherHomes: $atOtherHomes)
                    }");

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
        var document = Parser.ParseDocument(
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
                }");

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
        var document = Parser.ParseDocument(
            @"
            query variableIsNotDefined {
              dog {
                isHousetrained(atOtherHomes: $atOtherHomes)
              }
            }
             ");

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
        var document = Parser.ParseDocument(
            @"
            query variableIsNotDefinedUsedInSingleFragment {
              dog {
                ...isHousetrainedFragment
              }
            }

            fragment isHousetrainedFragment on Dog {
              isHousetrained(atOtherHomes: $atOtherHomes)
            }
             ");

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
        var document = Parser.ParseDocument(
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
             ");

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
        var document = Parser.ParseDocument(
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
             ");

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
        var document = Parser.ParseDocument(
            @"
                query variableUnused($atOtherHomes: Boolean) {
                  dog {
                    isHousetrained
                  }
                }
                 ");

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
        var document = Parser.ParseDocument(
            @"
                query variableNotUsedWithinFragment($atOtherHomes: Boolean) {
                  dog {
                    ...isHousetrainedWithoutVariableFragment
                  }
                }

                fragment isHousetrainedWithoutVariableFragment on Dog {
                  isHousetrained
                }
                 ");

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
        var document = Parser.ParseDocument(
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
                 ");

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
        var document = Parser.ParseDocument(
            @"
                query variableUsedInFragment($atOtherHomes: Boolean) {
                  dog {
                    ...isHousetrainedFragment
                  }
                }

                fragment isHousetrainedFragment on Dog {
                  isHousetrained(atOtherHomes: $atOtherHomes)
                }
                 ");

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
        var document = Parser.ParseDocument(
            @"
                query intCannotGoIntoBoolean($intArg: Int) {
                  arguments {
                    booleanArgField(booleanArg: $intArg)
                  }
                }
                ");

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
        var document = Parser.ParseDocument(
            @"
                query booleanListCannotGoIntoBoolean($booleanListArg: [Boolean]) {
                  arguments {
                    booleanArgField(booleanArg: $booleanListArg)
                  }
                }
                ");

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
        var document = Parser.ParseDocument(
            @"
                query booleanArgQuery($booleanArg: Boolean) {
                  arguments {
                    nonNullBooleanArgField(nonNullBooleanArg: $booleanArg)
                  }
                }
                ");

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
        var document = Parser.ParseDocument(
            @"
                query nonNullListToList($nonNullBooleanList: [Boolean]!) {
                  arguments {
                    booleanListArgField(booleanListArg: $nonNullBooleanList)
                  }
                }
                ");

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
        var document = Parser.ParseDocument(
            @"
                query listToNonNullList($booleanList: [Boolean]) {
                  arguments {
                    nonNullBooleanListField(nonNullBooleanListArg: $booleanList)
                  }
                }
                ");

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
        var document = Parser.ParseDocument(
            @"
                query booleanArgQueryWithDefault($booleanArg: Boolean) {
                  arguments {
                    optionalNonNullBooleanArgField(optionalBooleanArg: $booleanArg)
                  }
                }
                ");

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
        var document = Parser.ParseDocument(
            @"
                query booleanArgQueryWithDefault($booleanArg: Boolean = true) {
                  arguments {
                    nonNullBooleanArgField(nonNullBooleanArg: $booleanArg)
                  }
                }
                ");

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R585AllVariableUsagesAreAllowed());

        /* Then */
        Assert.True(result.IsValid);
    }
}