using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Experimental.ValueSerialization;
using Tanka.GraphQL.Language;
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

        public static object? CoerceValue(
            ExecutableSchema schema,
            object? value,
            TypeBase valueType,
            IReadOnlyDictionary<string, CoerceValue> valueConverters)
        {
            var typeDefinition = Ast.TypeFromAst(schema, valueType);

            if (typeDefinition == null)
                throw new InvalidOperationException(
                    $"Cannot coerce value. Given schema does not know value type '{valueType.ToGraphQL()}'.");

            if (!valueConverters.TryGetValue(typeDefinition.Name, out var converter))
                throw new InvalidOperationException(
                    $"Cannot coerce value. No value converter given for type '{typeDefinition.Name}'.");

            try
            {
                return converter(schema, value, valueType);
            }
            catch (Exception x)
            {
                // wrap exceptions in FormatException if needed
                if (x is not FormatException)
                    throw new FormatException(
                        $"Could not coerce value '{valueType.ToGraphQL()}' of type '{typeDefinition.Name}'. " +
                        "Coercing value resulted in error.", x);

                throw;
            }
        }
    }
}