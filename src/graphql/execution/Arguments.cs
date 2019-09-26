using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public static class Arguments
    {
        public static IReadOnlyDictionary<string, object> CoerceArgumentValues(
            ISchema schema,
            ObjectType objectType,
            GraphQLFieldSelection field, 
            IReadOnlyDictionary<string, object> coercedVariableValues)
        {
            var coercedValues = new Dictionary<string, object>();

            var argumentValues = field.Arguments?.ToList() ?? new List<GraphQLArgument>();
            var fieldName = field.Name.Value;
            var argumentDefinitions = schema.GetField(objectType.Name, fieldName).Arguments;

            if (argumentDefinitions == null)
                return coercedValues;

            foreach (var argumentDefinitionPair in argumentDefinitions)
            {
                var argumentDefinition = argumentDefinitionPair.Value;
                var argumentName = argumentDefinitionPair.Key;
                var argument = argumentValues.SingleOrDefault(a => a.Name.Value == argumentName);
                coercedValues[argumentName] = CoerceArgumentValue(
                    schema,
                    coercedVariableValues, 
                    argumentName,
                    argumentDefinition,
                    argument);
            }

            return coercedValues;
        }

        public static object CoerceArgumentValue(
            ISchema schema, 
            IReadOnlyDictionary<string, object> coercedVariableValues,
            string argumentName,
            Argument argumentDefinition, 
            GraphQLArgument argument)
        {
            
            var argumentType = argumentDefinition.Type;
            var defaultValue = argumentDefinition.DefaultValue;
            var argumentValue = argument?.Value;

            var hasValue = argumentValue != null;
            object value = null;

            if (argumentValue is GraphQLVariable variable)
            {
                if (coercedVariableValues == null)
                    hasValue = false;
                else
                {
                    var variableName = variable.Name.Value;
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
                if (value == null)
                {
                    return null;
                }

                if (argumentValue is GraphQLVariable)
                {
                    return value;
                }

                var coercedValue = Values.CoerceValue(
                    schema.GetInputFields,
                    value,
                    argumentType);

                return coercedValue;
            }

            return defaultValue;
        }
    }
}