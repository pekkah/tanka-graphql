using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Argument = Tanka.GraphQL.TypeSystem.Argument;

namespace Tanka.GraphQL.Execution
{
    public static class ArgumentCoercion
    {
        public static object? CoerceArgumentValue(
            ISchema schema,
            IReadOnlyDictionary<string, object>? coercedVariableValues,
            string argumentName,
            Argument argumentDefinition,
            Language.Nodes.Argument argument)
        {
            var argumentType = argumentDefinition.Type;
            var defaultValue = argumentDefinition.DefaultValue;
            var argumentValue = argument?.Value;

            var hasValue = argumentValue != null;
            object? value = null;

            if (argumentValue is Variable variable)
            {
                if (coercedVariableValues == null)
                {
                    hasValue = false;
                }
                else
                {
                    string variableName = variable.Name;
                    hasValue = coercedVariableValues.ContainsKey(variableName);
                    if (hasValue)
                        value = coercedVariableValues[variableName];
                }
            }
            else
            {
                value = argumentValue;
            }

            if (argumentType is NonNull && (!hasValue || value == null))
                throw new ValueCoercionException(
                    $"Argument '{argumentName}' is non-null but no value could be coerced",
                    null,
                    argumentType);

            if (hasValue)
            {
                if (value == null) return null;

                if (argumentValue is Variable) return value;

                var coercedValue = Values.CoerceValue(
                    schema.GetInputFields,
                    schema.GetValueConverter,
                    value,
                    argumentType);

                return coercedValue;
            }

            return defaultValue;
        }

        public static IReadOnlyDictionary<string, object?> CoerceArgumentValues(
            ISchema schema,
            ObjectType objectType,
            FieldSelection field,
            IReadOnlyDictionary<string, object> coercedVariableValues)
        {
            var coercedValues = new Dictionary<string, object?>();

            var argumentValues = field.Arguments ?? Arguments.None;
            var fieldName = field.Name;
            var argumentDefinitions = schema.GetField(objectType.Name, fieldName)
                .Arguments;

            if (argumentDefinitions == null)
                return coercedValues;

            foreach (var argumentDefinitionPair in argumentDefinitions)
            {
                var argumentDefinition = argumentDefinitionPair.Value;
                Name argumentName = argumentDefinitionPair.Key;
                var argument = argumentValues.SingleOrDefault(a => a.Name == argumentName);
                coercedValues[argumentName] = CoerceArgumentValue(
                    schema,
                    coercedVariableValues,
                    argumentName,
                    argumentDefinition,
                    argument);
            }

            return coercedValues;
        }
    }
}