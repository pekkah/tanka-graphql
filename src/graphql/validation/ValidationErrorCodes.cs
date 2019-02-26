namespace tanka.graphql.validation
{
    public static class ValidationErrorCodes
    {
        public const string R5211OperationNameUniqueness = "5.2.1.1 Operation Name Uniqueness";

        public const string R511ExecutableDefinitions = "5.1.1 Executable Definitions";

        public const string R5221LoneAnonymousOperation = "5.2.2.1 Lone Anonymous Operation";

        public const string R5231SingleRootField = "5.2.3.1 Single root field";

        public const string R531FieldSelections = "5.3.1 Field Selections on Objects, Interfaces, and Unions Types";

        public const string R533LeafFieldSelections = "5.3.3 Leaf Field Selections";

        public const string R541ArgumentNames = "5.4.1 Argument Names";
        
        public const string R542ArgumentUniqueness = "5.4.2 Argument Uniqueness";

        public const string R5421RequiredArguments = "5.4.2.1 Required Arguments";

        public const string R5511FragmentNameUniqueness = "5.5.1.1 Fragment Name Uniqueness";
        
        public const string R5512FragmentSpreadTypeExistence = "5.5.1.2 Fragment Spread Type Existence";

        public const string R5513FragmentsOnCompositeTypes = "5.5.1.3 Fragments On Composite Types";

        public const string R5514FragmentsMustBeUsed = "5.5.1.4 Fragments Must Be Used";

        public const string R5522FragmentSpreadsMustNotFormCycles = "5.5.2.2 Fragment spreads must not form cycles";
    }
}