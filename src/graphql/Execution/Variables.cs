using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public static class Variables
    {
        public static IReadOnlyDictionary<string, object> CoerceVariableValues(
            ISchema schema,
            GraphQLOperationDefinition operation, 
            Dictionary<string, object> variableValues)
        {
            var coercedValues = new Dictionary<string, object>();
            var variableDefinitions = operation.VariableDefinitions;

            if (variableDefinitions == null)
                return coercedValues;

            foreach (var variableDefinition in variableDefinitions)
            {
                var variableName = variableDefinition.Variable.Name.Value;
                var variableType = Ast.TypeFromAst(schema, variableDefinition.Type);

                //  should be assert?
                if (!TypeIs.IsInputType(variableType))
                    throw new VariableException($"Variable is not of input type", variableName, variableType);

                var defaultValue = variableDefinition.DefaultValue;
                var hasValue = variableValues.ContainsKey(variableName);
                var value = variableValues[variableName];

                if (!hasValue && defaultValue != null) 
                    coercedValues[variableName] = defaultValue;

                if (variableType is NonNull
                    && (!hasValue || value == null))
                    throw new ValueCoercionException(
                        $"Variable {variableName} type is non-nullable but value is null or not set",
                        value,
                        variableType);

                if (hasValue)
                {
                    if (value == null)
                        coercedValues[variableName] = null;
                    else
                        coercedValues[variableName] = Values.CoerceValue(
                            schema.GetInputFields,
                            schema.GetValueConverter,
                            value,
                            variableType);
                }
            }

            return coercedValues;
        }
    }
}