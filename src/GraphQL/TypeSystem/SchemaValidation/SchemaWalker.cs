using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public class SchemaWalker
{
    private readonly List<SchemaValidationRule> _rules;
    private readonly SchemaValidationResult _result;

    public SchemaWalker(IEnumerable<SchemaValidationRule> rules)
    {
        _result = new SchemaValidationResult();
        _rules = rules.ToList();
        
        // Set up error reporting for each rule
        foreach (var rule in _rules)
        {
            rule.SetErrorReporter(error => _result.AddError(error));
        }
    }

    public SchemaValidationResult Walk(IEnumerable<TypeDefinition> typeDefinitions)
    {
        foreach (var typeDefinition in typeDefinitions)
        {
            WalkTypeDefinition(typeDefinition);
        }

        return _result;
    }

    private void WalkTypeDefinition(TypeDefinition typeDefinition)
    {
        // Call general type definition validation
        foreach (var rule in _rules)
        {
            rule.ValidateTypeDefinition(typeDefinition);
        }

        // Call specific type validation based on type
        switch (typeDefinition)
        {
            case ObjectDefinition objectDefinition:
                WalkObjectDefinition(objectDefinition);
                break;
            case InterfaceDefinition interfaceDefinition:
                WalkInterfaceDefinition(interfaceDefinition);
                break;
            case InputObjectDefinition inputObjectDefinition:
                WalkInputObjectDefinition(inputObjectDefinition);
                break;
            case UnionDefinition unionDefinition:
                WalkUnionDefinition(unionDefinition);
                break;
            case EnumDefinition enumDefinition:
                WalkEnumDefinition(enumDefinition);
                break;
            case ScalarDefinition scalarDefinition:
                WalkScalarDefinition(scalarDefinition);
                break;
        }
    }

    private void WalkObjectDefinition(ObjectDefinition objectDefinition)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateObjectDefinition(objectDefinition);
        }

        // Walk fields
        if (objectDefinition.Fields != null)
        {
            foreach (var field in objectDefinition.Fields)
            {
                WalkFieldDefinition(field, objectDefinition);
            }
        }
    }

    private void WalkInterfaceDefinition(InterfaceDefinition interfaceDefinition)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateInterfaceDefinition(interfaceDefinition);
        }

        // Walk fields
        if (interfaceDefinition.Fields != null)
        {
            foreach (var field in interfaceDefinition.Fields)
            {
                WalkFieldDefinition(field, interfaceDefinition);
            }
        }
    }

    private void WalkInputObjectDefinition(InputObjectDefinition inputObjectDefinition)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateInputObjectDefinition(inputObjectDefinition);
        }

        // Walk input fields
        if (inputObjectDefinition.Fields != null)
        {
            foreach (var field in inputObjectDefinition.Fields)
            {
                WalkInputValueDefinition(field, inputObjectDefinition);
            }
        }
    }

    private void WalkUnionDefinition(UnionDefinition unionDefinition)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateUnionDefinition(unionDefinition);
        }
    }

    private void WalkEnumDefinition(EnumDefinition enumDefinition)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateEnumDefinition(enumDefinition);
        }
    }

    private void WalkScalarDefinition(ScalarDefinition scalarDefinition)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateScalarDefinition(scalarDefinition);
        }
    }

    private void WalkFieldDefinition(FieldDefinition fieldDefinition, TypeDefinition parentType)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateFieldDefinition(fieldDefinition, parentType);
        }

        // Walk field arguments
        if (fieldDefinition.Arguments != null)
        {
            foreach (var argument in fieldDefinition.Arguments)
            {
                WalkInputValueDefinition(argument, parentType);
            }
        }
    }

    private void WalkInputValueDefinition(InputValueDefinition inputValueDefinition, TypeDefinition parentType)
    {
        foreach (var rule in _rules)
        {
            rule.ValidateInputValueDefinition(inputValueDefinition, parentType);
        }
    }

}