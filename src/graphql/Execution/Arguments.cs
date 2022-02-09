using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public static class ArgumentCoercion
    {
        public static object? CoerceArgumentValue(
            ISchema schema,
            IReadOnlyDictionary<string, object?>? coercedVariableValues,
            string argumentName,
            InputValueDefinition argumentDefinition,
            Argument? argument)
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

            if (argumentType is NonNullType && (!hasValue || value == null))
                throw new ValueCoercionException(
                    $"Argument '{argumentName}' is non-null but no value could be coerced",
                    null,
                    argumentType);

            if (hasValue)
            {
                if (value == null) return null;

                if (argumentValue is Variable) return value;

                var coercedValue = Values.CoerceValue(
                    schema,
                    value,
                    argumentType);

                return coercedValue;
            }

            return defaultValue;
        }

        public static IReadOnlyDictionary<string, object?> CoerceArgumentValues(
            ISchema schema,
            ObjectDefinition objectDefinition,
            FieldSelection field,
            IReadOnlyDictionary<string, object?> coercedVariableValues)
        {
            var coercedValues = new Dictionary<string, object?>();

            var argumentValues = field.Arguments ?? Arguments.None;
            var fieldName = field.Name;
            var argumentDefinitions = schema.GetRequiredField(objectDefinition.Name, fieldName)
                .Arguments;

            if (argumentDefinitions == null)
                return coercedValues;

            foreach (var definition in argumentDefinitions)
            {
                var argumentDefinition = definition;
                Name argumentName = definition.Name;
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