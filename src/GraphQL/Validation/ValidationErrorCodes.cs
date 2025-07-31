namespace Tanka.GraphQL.Validation;

/// <summary>
///     References to sections of https://spec.graphql.org/draft/
/// </summary>
public static class ValidationErrorCodes
{
    public const string R511ExecutableDefinitions = "5.1.1 Executable Definitions";

    public const string R5211OperationNameUniqueness = "5.2.1.1 Operation Name Uniqueness";

    public const string R5221LoneAnonymousOperation = "5.2.2.1 Lone Anonymous Operation";

    public const string R5231SingleRootField = "5.2.3.1 Single root field";

    public const string R531FieldSelections = "5.3.1 Field Selections on Objects, Interfaces, and Unions Types";

    public const string R532FieldSelectionMerging = "5.3.2 Field Selection Merging";

    public const string R533LeafFieldSelections = "5.3.3 Leaf Field Selections";

    public const string R541ArgumentNames = "5.4.1 Argument Names";

    public const string R5421RequiredArguments = "5.4.2.1 Required Arguments";

    public const string R542ArgumentUniqueness = "5.4.2 Argument Uniqueness";

    public const string R5511FragmentNameUniqueness = "5.5.1.1 Fragment Name Uniqueness";

    public const string R5512FragmentSpreadTypeExistence = "5.5.1.2 Fragment Spread Type Existence";

    public const string R5513FragmentsOnCompositeTypes = "5.5.1.3 Fragments On Composite Types";

    public const string R5514FragmentsMustBeUsed = "5.5.1.4 Fragments Must Be Used";

    public const string R5521FragmentSpreadTargetDefined = "5.5.2.1 Fragment Spread Target Defined";

    public const string R5522FragmentSpreadsMustNotFormCycles = "5.5.2.2 Fragment spreads must not form cycles";

    public const string R5523FragmentSpreadIsPossible = "5.5.2.3 Fragment spread is possible";

    public const string R561ValuesOfCorrectType = "5.6.1 Values of Correct Type";

    public const string R562InputObjectFieldNames = "5.6.2 Input Object Field Names";

    public const string R563InputObjectFieldUniqueness = "5.6.3 Input Object Field Uniqueness";

    public const string R564InputObjectRequiredFields = "5.6.4 Input Object Required Fields";

    public const string R571DirectivesAreDefined = "5.7.1 Directives Are Defined";

    public const string R572DirectivesAreInValidLocations = "5.7.2 Directives Are In Valid Locations";

    public const string R573DirectivesAreUniquePerLocation = "5.7.3 Directives Are Unique Per Location";

    public const string R582VariablesAreInputTypes = "5.8.2 Variables Are Input Types";

    public const string R583AllVariableUsesDefined = "5.8.3 All Variable Uses Defined";

    public const string R584AllVariablesUsed = "5.8.4 All Variables Used";

    public const string R585AllVariableUsagesAreAllowed = "5.8.5 All Variable Usages Are Allowed";

    public static string R581VariableUniqueness = "5.8.1 Variable Uniqueness";
}