﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.execution;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.resolvers
{
    public class ResolveResult : IResolveResult
    {
        public ResolveResult(object value)
        {
            Value = value;
        }

        public ResolveResult(ObjectType objectType, object value)
            : this(value)
        {
            ActualType = objectType;
        }

        public ResolveResult(IEnumerable values)
        {
            Value = values;
        }

        public ResolveResult(IEnumerable<IResolveResult> values)
        {
            Value = values;
        }

        public object Value { get; protected set; }

        public ObjectType ActualType { get; set; }

        public virtual async Task<object> CompleteValueAsync(IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IType fieldType,
            GraphQLFieldSelection selection,
            List<GraphQLFieldSelection> fields,
            Dictionary<string, object> coercedVariableValues, 
            NodePath path)
        {
            object completedValue = null;

            completedValue = await CompleteValueAsync(
                executorContext,
                objectType,
                field,
                fieldType,
                ActualType,
                selection,
                fields,
                Value,
                coercedVariableValues,
                path).ConfigureAwait(false);

            return completedValue;
        }

        public async Task<object> CompleteValueAsync(IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IType fieldType,
            ObjectType actualType,
            GraphQLFieldSelection selection,
            List<GraphQLFieldSelection> fields,
            object value,
            Dictionary<string, object> coercedVariableValues, 
            NodePath path)
        {
            if (value is IResolveResult resolveResult)
                return await resolveResult.CompleteValueAsync(
                    executorContext,
                    objectType,
                    field,
                    fieldType,
                    selection,
                    fields,
                    coercedVariableValues,
                    path).ConfigureAwait(false);

            if (fieldType is NonNull nonNull)
            {
                var innerType = nonNull.WrappedType;
                var completedResult = await CompleteValueAsync(
                    executorContext,
                    objectType,
                    field,
                    innerType,
                    actualType,
                    selection,
                    fields,
                    value,
                    coercedVariableValues,
                    path).ConfigureAwait(false);

                if (completedResult == null)
                    throw new CompleteValueException(
                        $"Cannot complete value on non-null field '{selection.Name.Value}:{nonNull}'. " +
                        "Completed value is null.");

                return completedResult;
            }

            if (value == null)
                return null;


            if (fieldType is List listType)
            {
                if (!(value is IEnumerable values))
                    throw new CompleteValueException(
                        $"Cannot complete value for list field '{selection.Name.Value}':'{fieldType}'. " +
                        "Resolved value is not a collection");

                var innerType = listType.WrappedType;
                var result = new List<object>();
                int i = 0;
                foreach (var resultItem in values)
                {
                    var itemPath = path.Fork().Append(i++);
                    var completedResultItem = await CompleteValueAsync(
                        executorContext,
                        objectType,
                        field,
                        innerType,
                        actualType,
                        selection,
                        fields,
                        resultItem,
                        coercedVariableValues,
                        itemPath).ConfigureAwait(false);

                    result.Add(completedResultItem);
                }

                return result;
            }

            if (fieldType is ScalarType scalarType) return scalarType.Serialize(value);

            if (fieldType is EnumType enumType) return enumType.Serialize(value);

            if (fieldType is ObjectType fieldObjectType)
            {
                var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
                var data = await SelectionSets.ExecuteSelectionSetAsync(
                    executorContext,
                    subSelectionSet,
                    fieldObjectType,
                    value,
                    coercedVariableValues,
                    path).ConfigureAwait(false);

                return data;
            }

            // interfaces and unions require ActualType
            if (actualType == null)
                throw new CompleteValueException(
                    "Cannot complete value as interface or union. " +
                    $"Actual type not given from resolver. Use {nameof(Resolve.As)} with type parameter");

            if (fieldType is InterfaceType interfaceType)
            {
                if (!actualType.Implements(interfaceType))
                    throw new CompleteValueException(
                        "Cannot complete value as interface. " +
                        $"Actual type {actualType.Name} does not implement {interfaceType.Name}");

                var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
                var data = await SelectionSets.ExecuteSelectionSetAsync(
                    executorContext,
                    subSelectionSet,
                    actualType,
                    value,
                    coercedVariableValues,
                    path).ConfigureAwait(false);

                return data;
            }

            if (fieldType is UnionType unionType)
            {
                if (!unionType.IsPossible(actualType))
                    throw new CompleteValueException(
                        "Cannot complete value as union. " +
                        $"Actual type {actualType.Name} is not possible for {unionType.Name}");

                var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
                var data = await SelectionSets.ExecuteSelectionSetAsync(
                    executorContext,
                    subSelectionSet,
                    actualType,
                    value,
                    coercedVariableValues,
                    path).ConfigureAwait(false);

                return data;
            }

            throw new CompleteValueException($"Cannot complete value for field {field}. No handling for the type {fieldType}.");
        }
    }
}