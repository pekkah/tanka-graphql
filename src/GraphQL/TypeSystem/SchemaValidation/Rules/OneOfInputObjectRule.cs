using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation.Rules;

public class OneOfInputObjectRule : SchemaValidationRule
{
    public override void ValidateInputObjectDefinition(InputObjectDefinition inputObjectDefinition)
    {
        // Check if this input object has @oneOf directive
        if (!inputObjectDefinition.HasDirective("oneOf"))
            return;

        // Validate that @oneOf input objects have at least one field
        if (inputObjectDefinition.Fields is null || !inputObjectDefinition.Fields.Any())
        {
            ReportError(new SchemaValidationError(
                SchemaValidationErrorCodes.OneOfInputObjectNoFields,
                $"@oneOf input object '{inputObjectDefinition.Name}' must have at least one field",
                inputObjectDefinition));
            return;
        }

        // Validate that no fields have default values
        foreach (var field in inputObjectDefinition.Fields)
        {
            if (field.DefaultValue != null)
            {
                ReportError(new SchemaValidationError(
                    SchemaValidationErrorCodes.OneOfInputObjectDefaultValue,
                    $"@oneOf input object '{inputObjectDefinition.Name}' field '{field.Name}' cannot have a default value. @oneOf input objects cannot have fields with default values",
                    field));
            }
        }
    }
}