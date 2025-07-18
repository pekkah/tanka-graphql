using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Validation;

public partial class ValidatorFacts
{
    [Fact]
    public void Rule_531_Field_Selections_invalid_with_fragment()
    {
        /* Given */
        var document =
            @"fragment fieldNotDefined on Dog {
                      meowVolume
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R531FieldSelections);
    }

    [Fact]
    public void Rule_531_Field_Selections_invalid_with_alias()
    {
        /* Given */
        var document =
            @"fragment aliasedLyingFieldTargetNotDefined on Dog {
                      barkVolume: kawVolume
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R531FieldSelections);
    }

    [Fact]
    public void Rule_531_Field_Selections_valid()
    {
        /* Given */
        var document =
            @"{
                  dog {
                    name
                  }
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_531_Field_Selections_valid_with_interface()
    {
        /* Given */
        var document =
            @"fragment interfaceFieldSelection on Pet {
                  name
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_531_Field_Selections_invalid_with_interface()
    {
        /* Given */
        var document =
            @"fragment definedOnImplementorsButNotInterface on Pet {
                      nickname
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R531FieldSelections);
    }

    [Fact]
    public void Rule_531_Field_Selections_valid_with_union()
    {
        /* Given */
        var document =
            @"fragment inDirectFieldSelectionOnUnion on CatOrDog {
                  __typename
                  ... on Pet {
                    name
                  }
                  ... on Dog {
                    barkVolume
                  }
                }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_531_Field_Selections_invalid_with_union()
    {
        /* Given */
        var document =
            @"fragment directFieldSelectionOnUnion on CatOrDog {
                      name
                      barkVolume
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R531FieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R531FieldSelections
                     && error.Nodes.OfType<FieldSelection>()
                         .Any(n => n.Name == "name"));

        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R531FieldSelections
                     && error.Nodes.OfType<FieldSelection>()
                         .Any(n => n.Name == "barkVolume"));
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_valid1()
    {
        /* Given */
        var document =
            @"
                fragment mergeIdenticalFields on Dog {
                  name
                  name
                }

                fragment mergeIdenticalAliasesAndFields on Dog {
                  otherName: name
                  otherName: name
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_valid2()
    {
        /* Given */
        var document =
            @"
                fragment mergeIdenticalFieldsWithIdenticalArgs on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand(dogCommand: SIT)
                }

                fragment mergeIdenticalFieldsWithIdenticalValues on Dog {
                  doesKnowCommand(dogCommand: $dogCommand)
                  doesKnowCommand(dogCommand: $dogCommand)
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging(),
            new Dictionary<string, object>
            {
                ["dogCommand"] = "SIT"
            });

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_valid3()
    {
        /* Given */
        var document =
            @"
                fragment safeDifferingFields on Pet {
                  ... on Dog {
                    volume: barkVolume
                  }
                  ... on Cat {
                    volume: meowVolume
                  }
                }

                fragment safeDifferingArgs on Pet {
                  ... on Dog {
                    doesKnowCommand(dogCommand: SIT)
                  }
                  ... on Cat {
                    doesKnowCommand(catCommand: JUMP)
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_invalid1()
    {
        /* Given */
        var document =
            @"
                fragment conflictingBecauseAlias on Dog {
                  name: nickname
                  name
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R532FieldSelectionMerging
                     && error.Nodes.OfType<FieldSelection>()
                         .Any(n => n.Name == "name"));
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_invalid2()
    {
        /* Given */
        var document =
            @"
                fragment conflictingArgsOnValues on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand(dogCommand: HEEL)
                }

                fragment conflictingArgsValueAndVar on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand(dogCommand: $dogCommand)
                }

                fragment conflictingArgsWithVars on Dog {
                  doesKnowCommand(dogCommand: $varOne)
                  doesKnowCommand(dogCommand: $varTwo)
                }

                fragment differingArgs on Dog {
                  doesKnowCommand(dogCommand: SIT)
                  doesKnowCommand
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging(),
            new Dictionary<string, object>
            {
                ["dogCommand"] = "HEEL",
                ["varOne"] = "SIT",
                ["varTwo"] = "HEEL"
            });

        /* Then */
        Assert.False(result.IsValid);
        Assert.All(
            result.Errors,
            error => Assert.True(error.Code == ValidationErrorCodes.R532FieldSelectionMerging));
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_invalid3()
    {
        /* Given */
        var document =
            @"
                fragment conflictingDifferingResponses on Pet {
                  ... on Dog {
                    someValue: nickname
                  }
                  ... on Cat {
                    someValue: meowVolume
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.False(result.IsValid);
        Assert.All(
            result.Errors,
            error => Assert.True(error.Code == ValidationErrorCodes.R532FieldSelectionMerging));
    }

    [Fact]
    public void Rule_533_Leaf_Field_Selections_valid()
    {
        /* Given */
        var document =
            @"fragment scalarSelection on Dog {
                      barkVolume
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R533LeafFieldSelections());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_533_Leaf_Field_Selections_invalid1()
    {
        /* Given */
        var document =
            @"fragment scalarSelectionsNotAllowedOnInt on Dog {
                      barkVolume {
                        sinceWhen
                      }
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R533LeafFieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
    }

    [Fact]
    public void Rule_533_Leaf_Field_Selections_invalid2()
    {
        /* Given */
        var document =
            @"query directQueryOnObjectWithoutSubFields {
                      human
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R533LeafFieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
    }

    [Fact]
    public void Rule_533_Leaf_Field_Selections_invalid3()
    {
        /* Given */
        var document =
            @"query directQueryOnInterfaceWithoutSubFields {
                      pet
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R533LeafFieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
    }

    [Fact]
    public void Rule_533_Leaf_Field_Selections_invalid4()
    {
        /* Given */
        var document =
            @"query directQueryOnUnionWithoutSubFields {
                      catOrDog
                    }";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R533LeafFieldSelections());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R533LeafFieldSelections);
    }

    /// <summary>
    /// Complex fragment merging scenarios for field selection merging
    /// </summary>
    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_nested_fragments_valid()
    {
        /* Given */
        var document =
            @"
                query complexNestedFragments {
                  dog {
                    ...dogFragment
                    ...petFragment
                  }
                }

                fragment dogFragment on Dog {
                  name
                  barkVolume
                  ...ownerFragment
                }

                fragment petFragment on Pet {
                  name
                }

                fragment ownerFragment on Dog {
                  owner {
                    name
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_inline_fragments_with_different_types_valid()
    {
        /* Given */
        var document =
            @"
                query complexInlineFragments {
                  catOrDog {
                    ... on Cat {
                      name
                      nickname
                      meowVolume
                    }
                    ... on Dog {
                      name
                      nickname
                      barkVolume
                    }
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_fragments_with_arguments_conflict_invalid()
    {
        /* Given */
        var document =
            @"
                query complexFragmentsWithArgumentsConflict {
                  dog {
                    ...dogCommandFragment1
                    ...dogCommandFragment2
                  }
                }

                fragment dogCommandFragment1 on Dog {
                  doesKnowCommand(dogCommand: SIT)
                }

                fragment dogCommandFragment2 on Dog {
                  doesKnowCommand(dogCommand: DOWN)
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R532FieldSelectionMerging);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_deeply_nested_fragments_invalid()
    {
        /* Given */
        var document =
            @"
                query deeplyNestedFragmentsConflict {
                  dog {
                    ...level1Fragment
                    ...level1ConflictFragment
                  }
                }

                fragment level1Fragment on Dog {
                  name
                  ...level2Fragment
                }

                fragment level1ConflictFragment on Dog {
                  name: nickname
                }

                fragment level2Fragment on Dog {
                  barkVolume
                  owner {
                    ...level3Fragment
                  }
                }

                fragment level3Fragment on Human {
                  name
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R532FieldSelectionMerging);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_interface_fragments_valid()
    {
        /* Given */
        var document =
            @"
                query complexInterfaceFragments {
                  pet {
                    ...petFragment
                    ... on Dog {
                      barkVolume
                    }
                    ... on Cat {
                      meowVolume
                    }
                  }
                }

                fragment petFragment on Pet {
                  name
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_union_fragments_with_alias_conflict_invalid()
    {
        /* Given */
        var document =
            @"
                query unionFragmentsWithAliasConflict {
                  catOrDog {
                    ... on Cat {
                      sound: meowVolume
                    }
                    ... on Dog {
                      sound: barkVolume
                    }
                  }
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R532FieldSelectionMerging);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_multiple_fragments_with_variables_valid()
    {
        /* Given */
        var document =
            @"
                query multipleFragmentsWithVariables($dogCmd: DogCommand!) {
                  dog {
                    ...fragment1
                    ...fragment2
                  }
                }

                fragment fragment1 on Dog {
                  name
                  doesKnowCommand(dogCommand: $dogCmd)
                }

                fragment fragment2 on Dog {
                  nickname
                  doesKnowCommand(dogCommand: $dogCmd)
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging(),
            new Dictionary<string, object> { ["dogCmd"] = "SIT" });

        /* Then */
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Rule_532_Field_Selection_Merging_complex_cross_fragment_field_conflicts_invalid()
    {
        /* Given */
        var document =
            @"
                query crossFragmentFieldConflicts {
                  dog {
                    ...conflictFragment1
                    ...conflictFragment2
                  }
                }

                fragment conflictFragment1 on Dog {
                  name
                  nickname
                }

                fragment conflictFragment2 on Dog {
                  name: nickname
                  barkVolume
                }
                ";

        /* When */
        var result = Validate(
            document,
            ExecutionRules.R532FieldSelectionMerging());

        /* Then */
        Assert.False(result.IsValid);
        Assert.Single(
            result.Errors,
            error => error.Code == ValidationErrorCodes.R532FieldSelectionMerging);
    }
}