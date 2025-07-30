using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Provides the @oneOf directive implementation as specified in the GraphQL RFC.
/// The @oneOf directive is applied to input types to indicate that exactly one
/// of the fields must be provided.
/// </summary>
public static class OneOfDirective
{
    /// <summary>
    /// The @oneOf directive definition as specified in the GraphQL specification.
    /// </summary>
    public static readonly DirectiveDefinition Directive =
        $"directive @oneOf on {TypeSystemDirectiveLocations.INPUT_OBJECT}";

    /// <summary>
    /// Creates a validation rule for @oneOf directive that ensures exactly one field
    /// is provided in oneOf input objects.
    /// </summary>
    /// <returns>A validation rule for @oneOf directive</returns>
    public static CombineRule ValidationRule()
    {
        return (context, rule) =>
        {
            // Handle validation for variables
            rule.EnterArgument += argument =>
            {
                if (argument.Value.Kind != NodeKind.Variable)
                    return;

                InputValueDefinition? argumentDefinition = context.Tracker.ArgumentDefinition;
                if (argumentDefinition is null)
                    return;

                if (context.Schema.GetNamedType(argumentDefinition.Type.Unwrap().Name) is not InputObjectDefinition
                    inputObject)
                    return;

                if (!inputObject.HasDirective(Directive.Name))
                    return;

                var variable = (Variable)argument.Value;
                if (context.VariableValues is null || !context.VariableValues.TryGetValue(variable.Name, out object? variableValue)) return;

                if (variableValue is not null)
                {
                    var coercedValue = Values.CoerceValue(
                        context.Schema,
                        variableValue,
                        argumentDefinition.Type) as IReadOnlyDictionary<string, object?>;

                    if (coercedValue?.Count(kv => kv.Value is not null) != 1)
                        context.Error("ONEOF001",
                            $"Invalid value for '@oneOf' input '{inputObject.Name}'. @oneOf input objects can only have one field value set.");
                }
            };

            // Handle validation for object literals (including nested ones)
            rule.EnterObjectValue += objectValue =>
            {
                if (context.Tracker.InputType is not { } inputType)
                    return;

                if (context.Schema.GetNamedType(inputType.Name) is not InputObjectDefinition inputObject)
                    return;

                if (!inputObject.HasDirective(Directive.Name))
                    return;

                // Check if exactly one field is set and not null
                var nonNullFields = objectValue.Count(field => field.Value.Kind != NodeKind.NullValue);

                if (nonNullFields != 1)
                    context.Error("ONEOF001",
                        $"Invalid value for '@oneOf' input '{inputObject.Name}'. @oneOf input objects can only have one field value set.");
            };
        };
    }
}