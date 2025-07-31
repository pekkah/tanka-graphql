using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public abstract class SchemaValidationRule
{
    protected Action<SchemaValidationError> ReportError { get; private set; } = _ => { };

    internal void SetErrorReporter(Action<SchemaValidationError> reportError)
    {
        ReportError = reportError;
    }

    public virtual void ValidateTypeDefinition(TypeDefinition typeDefinition) { }
    public virtual void ValidateObjectDefinition(ObjectDefinition objectDefinition) { }
    public virtual void ValidateInterfaceDefinition(InterfaceDefinition interfaceDefinition) { }
    public virtual void ValidateInputObjectDefinition(InputObjectDefinition inputObjectDefinition) { }
    public virtual void ValidateUnionDefinition(UnionDefinition unionDefinition) { }
    public virtual void ValidateEnumDefinition(EnumDefinition enumDefinition) { }
    public virtual void ValidateScalarDefinition(ScalarDefinition scalarDefinition) { }
    public virtual void ValidateFieldDefinition(FieldDefinition fieldDefinition, TypeDefinition parentType) { }
    public virtual void ValidateInputValueDefinition(InputValueDefinition inputValueDefinition, TypeDefinition parentType) { }
}