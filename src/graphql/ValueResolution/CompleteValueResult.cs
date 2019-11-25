using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public class CompleteValueResult : IResolverResult
    {
        private readonly ObjectType _actualType;
        private readonly object _value;

        public CompleteValueResult(object value, ObjectType actualType)
        {
            _value = value;
            _actualType = actualType;
        }

        public ValueTask<object> CompleteValueAsync(
            IResolverContext context)
        {
            return CompleteValueAsync(_value, context.Field.Type, context.Path, context);
        }

        private ValueTask<object> CompleteValueAsync(
            object value,
            IType type,
            NodePath path,
            IResolverContext context)
        {
            if (type is NonNull nonNull)
                return CompleteNonNullValueAsync(value, nonNull, path, context);

            if (value == null)
                return default;

            if (type is List list)
                return CompleteListValueAsync(value, list, path, context);

            return type switch
            {
                ScalarType scalarType => new ValueTask<object>(scalarType.Serialize(value)),
                EnumType enumType => new ValueTask<object>(enumType.Serialize(value)),
                ObjectType objectType => CompleteObjectValueAsync(value, objectType, path, context),

                InterfaceType interfaceType => CompleteInterfaceValueAsync(value, interfaceType, path, context),
                UnionType unionType => default,
                _ => throw new CompleteValueException(
                    $"Cannot complete value for field {context.FieldName}. Cannot complete value of type {type}.",
                    path,
                    context.Selection)
            };
        }

        private async Task<object> CompleteUnionValueAsync(
            object value,
            UnionType unionType,
            NodePath path,
            IResolverContext context)
        {
            if (_actualType == null)
                throw new CompleteValueException(
                    "Cannot complete value as interface or union. " +
                    "Actual type not given when resolving interface value.",
                    path,
                    context.Selection);

            if (!unionType.IsPossible(_actualType))
                throw new CompleteValueException(
                    "Cannot complete value as union. " +
                    $"Actual type {_actualType.Name} is not possible for {unionType.Name}",
                    path,
                    context.Selection);

            var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context.ExecutionContext,
                subSelectionSet,
                _actualType,
                value,
                path).ConfigureAwait(false);

            return data;
        }

        private async ValueTask<object> CompleteInterfaceValueAsync(
            object value,
            InterfaceType interfaceType,
            NodePath path,
            IResolverContext context)
        {
            if (_actualType == null)
                throw new CompleteValueException(
                    "Cannot complete value as interface or union. " +
                    "Actual type not given when resolving interface value.",
                    path,
                    context.Selection);

            if (!_actualType.Implements(interfaceType))
                throw new CompleteValueException(
                    "Cannot complete value as interface. " +
                    $"Actual type {_actualType.Name} does not implement {interfaceType.Name}",
                    path,
                    context.Selection);

            var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context.ExecutionContext,
                subSelectionSet,
                _actualType,
                value,
                path);

            return data;
        }

        private static async ValueTask<object> CompleteObjectValueAsync(
            object value,
            ObjectType objectType,
            NodePath path,
            IResolverContext context)
        {
            var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context.ExecutionContext,
                subSelectionSet,
                objectType,
                value,
                path);

            return data;
        }

        private async ValueTask<object> CompleteNonNullValueAsync(
            object value,
            NonNull nonNull,
            NodePath path,
            IResolverContext context)
        {
            var innerType = nonNull.OfType;
            var completedResult = await CompleteValueAsync(value, innerType, path, context);

            if (completedResult == null)
                throw new NullValueForNonNullException(
                    context.ObjectType.Name,
                    context.FieldName,
                    context.Path,
                    context.Selection);

            return completedResult;
        }

        private async ValueTask<object> CompleteListValueAsync(
            object value,
            List list,
            NodePath path,
            IResolverContext context)
        {
            if (!(value is IEnumerable values))
                throw new CompleteValueException(
                    $"Cannot complete value for list field '{context.FieldName}':'{listType}'. " +
                    "Resolved value is not a collection",
                    context.Path,
                    context.Selection);

            var innerType = list.OfType;
            var result = new List<object>();
            var i = 0;
            foreach (var resultItem in values)
            {
                var itemPath = path.Fork().Append(i++);

                try
                {
                    var completedResultItem = await
                        CompleteValueAsync(resultItem, innerType, itemPath, context);

                    result.Add(completedResultItem);
                }
                catch (Exception e)
                {
                    if (innerType is NonNull) throw;

                    context.ExecutionContext.AddError(e);
                    result.Add(null);
                }
            }

            return result;
        }
    }
}