using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Core
{
    public partial class OperationCore
    {
        public static async Task<object?> ExecuteField(
            OperationContext context,
            ObjectDefinition objectDefinition,
            object? objectValue,
            TypeBase fieldType,
            IReadOnlyList<FieldSelection> fields,
            NodePath path,
            CoerceArgumentValues coerceArgumentValues,
            ResolveFieldValue resolveFieldValue,
            CompleteValue completeValue,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            object? completedValue = null;

            try
            {
                var field = fields.First();
                var fieldName = field.Name;

                var argumentValues = await coerceArgumentValues(
                    context.Schema,
                    objectDefinition,
                    field,
                    context.CoercedVariableValues,
                    cancellationToken);

                var (resolvedValue, resolveAbstractType) = await resolveFieldValue(
                    context,
                    objectDefinition,
                    objectValue,
                    fieldName,
                    argumentValues,
                    path,
                    cancellationToken);

                completedValue = await completeValue(
                    context,
                    fieldType,
                    fields,
                    resolvedValue,
                    resolveAbstractType,
                    path,
                    cancellationToken);

                return completedValue;
            }
            catch (Exception error)
            {
                var fieldError = error as FieldException ?? new FieldException(
                    error.Message,
                    path,
                    fields
                        .Where(f => f.Location.HasValue)
                        .Select(f => f.Location!.Value)
                        .ToArray(),
                    error);

                // bubble to field above
                if (fieldType.Kind == NodeKind.NonNullType)
                    // ReSharper disable once PossibleIntendedRethrow
                    throw fieldError;

                context.AddError(fieldError);
                return completedValue;
            }
        }

        public static async Task<object?> CompleteValue(
            OperationContext context,
            TypeBase fieldType,
            IReadOnlyList<FieldSelection> fields,
            object? result,
            NodePath path,
            SerializeValue serializeValue,
            ResolveAbstractType? resolveAbstractType,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (fieldType is NonNullType nonNullType)
            {
                var innerType = nonNullType.OfType;
                var completedResult = await CompleteValue(
                    context,
                    innerType,
                    fields,
                    result,
                    path,
                    serializeValue,
                    resolveAbstractType,
                    cancellationToken);

                if (completedResult is null)
                    //todo: throw field error
                    throw new Exception("Field value cannot be null. Field is non-null type.");

                return completedResult;
            }

            if (result is null)
                return null;

            if (fieldType is ListType listType)
            {
                var innerType = listType.OfType;

                return result switch
                {
                    IEnumerable enumerableValue => await CompleteEnumerableValue(innerType, enumerableValue),
                    //todo: async enumerable support?
                    //todo: throw field error
                    _ => throw new Exception(
                        $"Cannot complete list value. Resolved value is not known enumerable type. Value type is '{result.GetType().Name}'")
                };
            }

            var namedType = (NamedType) fieldType;

            var typeDefinition = context.Schema.GetNamedType<TypeDefinition>(namedType.Name);

            if (typeDefinition is ScalarDefinition scalarDefinition)
                return await CompleteScalarValue(result);

            if (typeDefinition is EnumDefinition enumDefinition)
                return await CompleteEnumValue(result);

            var objectDefinition = typeDefinition switch
            {
                InterfaceDefinition interfaceDefinition => resolveAbstractType?.Invoke(
                    context.Schema,
                    interfaceDefinition,
                    result),
                UnionDefinition unionDefinition => resolveAbstractType?.Invoke(
                    context.Schema,
                    unionDefinition,
                    result),
                _ => typeDefinition as ObjectDefinition
            };

            if (objectDefinition is null)
                throw new Exception($"Cannot complete field value. FieldType '{fieldType.Kind}' cannot be completed.");

            var subSelectionSet = fields.Merge();
            return await context.ExecuteSelectionSet(
                context,
                objectDefinition,
                result,
                subSelectionSet,
                path,
                cancellationToken
            );

            async Task<IReadOnlyCollection<object?>> CompleteEnumerableValue(TypeBase innerType, IEnumerable value)
            {
                List<object?> completeValues = new();
                var enumerator = value.GetEnumerator();
                var count = 0;
                while (enumerator.MoveNext())
                {
                    var itemPath = path.Fork().Append(count);
                    var completedItem = await CompleteValue(
                        context,
                        innerType,
                        fields,
                        enumerator.Current,
                        itemPath,
                        serializeValue,
                        resolveAbstractType,
                        cancellationToken);

                    completeValues.Add(completedItem);
                    count++;
                }

                return completeValues;
            }

            ValueTask<object?> CompleteScalarValue(object value)
            {
                return serializeValue(context.Schema, scalarDefinition, value);
            }

            ValueTask<object?> CompleteEnumValue(object value)
            {
                return serializeValue(context.Schema, enumDefinition, value);
            }
        }
    }
}