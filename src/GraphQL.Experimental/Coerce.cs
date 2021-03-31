using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public class Coerce
    {
        public static ValueTask<IReadOnlyDictionary<string, object?>> CoerceVariableValues(
            ExecutableSchema schema,
            OperationDefinition operation,
            IReadOnlyDictionary<string, object?>? variableValues,
            CoerceValue coerceValue,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var coercedValues = new Dictionary<string, object?>();
            var variableDefinitions = operation?.VariableDefinitions;

            if (variableDefinitions == null)
                return new ValueTask<IReadOnlyDictionary<string, object?>>(coercedValues);

            foreach (var variableDefinition in variableDefinitions)
            {
                var variableName = variableDefinition.Variable.Name.Value;
                var variableType = variableDefinition.Type;

                if (!Ast.IsInputType(schema, variableType))
                    throw new InvalidOperationException(
                        $"Type '{variableType}' of '{variableName}' is not an input type");

                var defaultValue = variableDefinition.DefaultValue;
                object? value = null;
                var hasValue = variableValues?.TryGetValue(variableName, out value) == true;

                if (!hasValue && defaultValue != null)
                {
                    coercedValues.Add(variableName, defaultValue);
                }
                else if (variableType.Kind == NodeKind.NonNullType
                         && (!hasValue || value is null))
                    //todo: throw query error?
                {
                    throw new Exception($"No value for variable '{variableName}' given or value is null.");
                }
                else if (hasValue)
                {
                    if (value is null)
                        coercedValues.Add(variableName, null);
                    else
                        try
                        {
                            var coercedValue = coerceValue(
                                schema,
                                value,
                                variableType);

                            coercedValues.Add(variableName, coercedValue);
                        }
                        catch (Exception x)
                        {
                            //todo: throw query error?
                            throw new Exception(
                                $"Could not coerce value of variable '{variableName}'. Input coercion failed.", x);
                        }
                }
            }

            return new ValueTask<IReadOnlyDictionary<string, object?>>(coercedValues);
        }

        public static ValueTask<IReadOnlyDictionary<string, object?>> CoerceArgumentValues(
            ExecutableSchema schema,
            ObjectDefinition objectDefinition,
            FieldSelection field,
            CoerceValue coerceValue,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var coercedValues = new Dictionary<string, object?>();

            var argumentValues = field.Arguments;
            var fieldName = field.Name;
            var argumentDefinitions = schema.GetField(objectDefinition, fieldName)
                ?.Arguments;

            if (argumentDefinitions == null)
                return new ValueTask<IReadOnlyDictionary<string, object?>>(coercedValues);

            foreach (var argumentDefinition in argumentDefinitions)
            {
                var argumentName = argumentDefinition.Name.Value;
                var argumentType = argumentDefinition.Type;
                var defaultValue = argumentDefinition.DefaultValue;

                var argumentValue = argumentValues?.SingleOrDefault(a => a.Name.Value == argumentName);
                var hasValue = argumentValue != null;

                object? value = null;
                if (argumentValue?.Value?.Kind == NodeKind.Variable)
                {
                    var variableName = argumentValue.Name.Value;
                    hasValue = coercedVariableValues.TryGetValue(variableName, out value);
                }

                value = argumentValue?.Value;

                if (!hasValue && defaultValue != null)
                {
                    coercedValues.Add(argumentName, value);
                }
                else if (argumentType.Kind == NodeKind.NonNullType && (!hasValue || value is null))
                    //todo: throw query error?
                {
                    throw new Exception($"No value for argument '{argumentName}' given or value is null.");
                }
                else if (hasValue)
                {
                    if (value is null)
                        coercedValues.Add(argumentName, null);
                    else if (argumentValue?.Value?.Kind == NodeKind.Variable)
                        coercedValues.Add(argumentName, value);
                    else
                        try
                        {
                            var coercedValue = coerceValue(
                                schema,
                                value,
                                argumentType);

                            coercedValues.Add(argumentName, coercedValue);
                        }
                        catch (Exception x)
                        {
                            //todo: throw query error?
                            throw new Exception(
                                $"Could not coerce value of argument '{argumentValue}'. Input coercion failed.", x);
                        }
                }
            }


            return new ValueTask<IReadOnlyDictionary<string, object?>>(coercedValues);
        }

        public static object? CoerceValue(ExecutableSchema schema, object? value, TypeBase valueType)
        {
            return null;
        }
    }
}