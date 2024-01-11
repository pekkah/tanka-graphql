using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Extensions.Experimental.OneOf;

public class OneOfDirective
{
    private static readonly List<NodeKind> AllowedKinds = [NodeKind.ObjectValue, NodeKind.Variable];

    public static DirectiveDefinition Directive =>
        $"directive @oneOf on {TypeSystemDirectiveLocations.INPUT_OBJECT}";

    public static CombineRule OneOfValidationRule()
    {
        return (context, rule) =>
        {
            rule.EnterArgument += argument =>
            {
                InputValueDefinition? argumentDefinition = context.Tracker.ArgumentDefinition;

                if (argumentDefinition is null)
                    return;

                if (!AllowedKinds.Contains(argument.Value.Kind))
                    return;

                if (context.Schema.GetNamedType(argumentDefinition.Type.Unwrap().Name) is not InputObjectDefinition
                    inputObject)
                    return;

                if (!inputObject.HasDirective(Directive.Name))
                    return;

                if (argument.Value.Kind == NodeKind.Variable)
                {
                    var variable = (Variable)argument.Value;

                    if (context.VariableValues?.TryGetValue(variable.Name, out object? variableValue) != true) return;
                    
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
                }
                else
                {
                    var objectValue = (ObjectValue)argument.Value;

                    if (objectValue.Count != 1)
                        context.Error("ONEOF001",
                            $"Invalid value for '@oneOf' input '{inputObject.Name}'. @oneOf input objects can only have one field value set.");
                }
            };
        };
    }
}