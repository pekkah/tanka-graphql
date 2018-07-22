using System.Collections.Generic;
using System.Linq;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Arguments
    {
        public static Dictionary<string, object> CoerceArgumentValues(ObjectType objectType,
            GraphQLFieldSelection field, Dictionary<string, object> coercedVariableValues)
        {
            var coercedValues = new Dictionary<string, object>();

            var argumentValues = field.Arguments?.ToList() ?? new List<GraphQLArgument>();
            var fieldName = field.Name.Value;
            var argumentDefinitions = objectType.GetField(fieldName).Arguments;

            if (argumentDefinitions == null)
                return coercedValues;

            foreach (var argumentDefinitionPair in argumentDefinitions)
            {
                var argumentDefinition = argumentDefinitionPair.Value;
                var argumentName = argumentDefinitionPair.Key;
                var argumentType = argumentDefinition.Type;
                var defaultValue = argumentDefinition.DefaultValue;

                var argument = argumentValues.SingleOrDefault(a => a.Name.Value == argumentName);
                var argumentValue = argument?.Value;
                var hasValue = argumentValue != null;
                object value = null;

                if (argumentValue is GraphQLVariable variable)
                {
                    var variableName = variable.Name.Value;
                    hasValue = coercedVariableValues.ContainsKey(variableName);
                    if (hasValue)
                        value = coercedVariableValues[variableName];
                }
                else
                {
                    value = argumentValue;
                }

                if (!hasValue) coercedValues[argumentName] = defaultValue;

                if (argumentType is NonNull && (!hasValue || value == null))
                    throw new NullValueForNonNullTypeException(
                        $"Argument {argumentName} is non-null but no value could be coerced",
                        argumentType);

                if (hasValue)
                {
                    if (value == null)
                    {
                        coercedValues[argumentName] = null;
                    }
                    else if (argumentValue is GraphQLVariable)
                    {
                        coercedValues[argumentName] = value;
                    }
                    else
                    {
                        var coercedValue = Values.CoerceValue(
                            value,
                            argumentType);

                        coercedValues[argumentName] = coercedValue;
                    }
                }
            }

            return coercedValues;
        }
    }
}