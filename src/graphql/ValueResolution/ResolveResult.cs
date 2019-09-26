using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
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
            IReadOnlyCollection<GraphQLFieldSelection> fields,
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
                path).ConfigureAwait(false);

            return completedValue;
        }

        public async Task<object> CompleteValueAsync(IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IType fieldType,
            ObjectType actualType,
            GraphQLFieldSelection selection,
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            object value,
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
                    path).ConfigureAwait(false);

            if (fieldType is NonNull nonNull)
            {
                return await CompleteNonNullValueAsync(
                    executorContext, 
                    objectType,
                    field, 
                    actualType, 
                    selection, 
                    fields,
                    value,
                    path, 
                    nonNull);
            }

            if (value == null)
                return null;

            if (fieldType is List listType)
            {
                return await CompleteListValueAsync(
                    executorContext, 
                    objectType,
                    field, 
                    fieldType, 
                    actualType, 
                    selection, 
                    fields, 
                    value,
                    path, 
                    listType);
            }

            if (fieldType is ScalarType scalarType) return scalarType.Serialize(value);

            if (fieldType is EnumType enumType) return enumType.Serialize(value);

            if (fieldType is ObjectType fieldObjectType)
            {
                return await CompleteObjectValueAsync(
                    executorContext,
                    fields, 
                    value, 
                    path, 
                    fieldObjectType);
            }

            // interfaces and unions require ActualType
            if (actualType == null)
                throw new CompleteValueException(
                    "Cannot complete value as interface or union. " +
                    $"Actual type not given from resolver. Use {nameof(Resolve.As)} with type parameter",
                    path,
                    fields.First());

            if (fieldType is InterfaceType interfaceType)
            {
                return await CompleteInterfaceValueAsync(
                    executorContext, 
                    actualType, 
                    fields, 
                    value,
                    path, 
                    interfaceType);
            }

            if (fieldType is UnionType unionType)
            {
                return await CompleteUnionValueAsync(
                    executorContext,
                    actualType, 
                    fields, 
                    value, 
                    path, 
                    unionType);
            }

            throw new CompleteValueException(
                $"Cannot complete value for field {field}. No handling for the type {fieldType}.",
                path,
                fields.First());
        }

        private static async Task<object> CompleteUnionValueAsync(
            IExecutorContext executorContext, 
            ObjectType actualType, 
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            object value,
            NodePath path, 
            UnionType unionType)
        {
            if (!unionType.IsPossible(actualType))
                throw new CompleteValueException(
                    "Cannot complete value as union. " +
                    $"Actual type {actualType.Name} is not possible for {unionType.Name}",
                    path,
                    fields.First());

            var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                executorContext,
                subSelectionSet,
                actualType,
                value,
                path).ConfigureAwait(false);

            return data;
        }

        private static async Task<object> CompleteInterfaceValueAsync(
            IExecutorContext executorContext, 
            ObjectType actualType,
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            object value, 
            NodePath path, 
            InterfaceType interfaceType)
        {
            if (!actualType.Implements(interfaceType))
                throw new CompleteValueException(
                    "Cannot complete value as interface. " +
                    $"Actual type {actualType.Name} does not implement {interfaceType.Name}",
                    path,
                    fields.First());

            var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                executorContext,
                subSelectionSet,
                actualType,
                value,
                path).ConfigureAwait(false);

            return data;
        }

        private static async Task<object> CompleteObjectValueAsync(
            IExecutorContext executorContext, 
            IReadOnlyCollection<GraphQLFieldSelection> fields, 
            object value,
            NodePath path, 
            ObjectType fieldObjectType)
        {
            var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                executorContext,
                subSelectionSet,
                fieldObjectType,
                value,
                path).ConfigureAwait(false);

            return data;
        }

        private async Task<object> CompleteListValueAsync(
            IExecutorContext executorContext, 
            ObjectType objectType, 
            IField field,
            IType fieldType, 
            ObjectType actualType, 
            GraphQLFieldSelection selection, 
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            object value,
            NodePath path, 
            List listType)
        {
            if (!(value is IEnumerable values))
                throw new CompleteValueException(
                    $"Cannot complete value for list field '{selection.Name.Value}':'{fieldType}'. " +
                    "Resolved value is not a collection",
                    path,
                    selection);

            var innerType = listType.OfType;
            var result = new List<object>();
            int i = 0;
            foreach (var resultItem in values)
            {
                var itemPath = path.Fork().Append(i++);
                try
                {
                    var completedResultItem = await CompleteValueAsync(
                        executorContext,
                        objectType,
                        field,
                        innerType,
                        actualType,
                        selection,
                        fields,
                        resultItem,
                        itemPath).ConfigureAwait(false);

                    result.Add(completedResultItem);
                }
                catch (Exception e)
                {
                    if (innerType is NonNull)
                    {
                        throw;
                    }

                    executorContext.AddError(e);
                    result.Add(null);
                }
            }

            return result;
        }

        private async Task<object> CompleteNonNullValueAsync(
            IExecutorContext executorContext, 
            ObjectType objectType, 
            IField field,
            ObjectType actualType, 
            GraphQLFieldSelection selection, 
            IReadOnlyCollection<GraphQLFieldSelection> fields, 
            object value,
            NodePath path,
            NonNull nonNull)
        {
            var innerType = nonNull.OfType;
            var completedResult = await CompleteValueAsync(
                executorContext,
                objectType,
                field,
                innerType,
                actualType,
                selection,
                fields,
                value,
                path).ConfigureAwait(false);

            if (completedResult == null)
                throw new NullValueForNonNullException(
                    objectType.Name,
                    selection.Name.Value,
                    path,
                    selection);

            return completedResult;
        }
    }
}