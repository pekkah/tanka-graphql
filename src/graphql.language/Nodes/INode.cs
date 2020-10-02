namespace Tanka.GraphQL.Language.Nodes
{
    public interface INode
    {
       public NodeKind Kind { get; }
       public Location? Location { get; }
    }

    public enum NodeKind
    {
        Argument,
        BooleanValue,
        DefaultValue,
        Directive,
        EnumValue,
        ExecutableDocument,
        FieldSelection,
        FloatValue,
        FragmentDefinition,
        FragmentSpread,
        InlineFragment,
        IntValue,
        ListValue,
        NamedType,
        NonNullType,
        NullValue,
        ObjectValue,
        OperationDefinition,
        SelectionSet,
        StringValue,
        Variable,
        VariableDefinition,
        DirectiveDefinition,
        EnumDefinition,
        InputObjectDefinition,
        InterfaceDefinition,
        ObjectDefinition,
        ScalarDefinition,
        SchemaDefinition,
        SchemaExtension,
        TypeSystemDocument,
        UnionDefinition,
        ListType,
        ObjectField,
        EnumValueDefinition,
        FieldDefinition,
        InputValueDefinition,
        TypeExtension,
        TankaImport,

        Directives,
        Arguments,
        FragmentDefinitions,
        OperationDefinitions,
        VariableDefinitions,
        ArgumentsDefinition,
        EnumValuesDefinition,
        ImplementsInterfaces,
        FieldsDefinition,
        RootOperationTypeDefinition,
        UnionMemberTypes,
        InputFieldsDefinition
    }
}