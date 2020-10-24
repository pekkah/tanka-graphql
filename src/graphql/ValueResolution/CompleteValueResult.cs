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
        private readonly object? _value;

        public CompleteValueResult(object? value, IType actualType)
        {
            _value = value;
            IsTypeOf = _ => actualType;
        }

        public CompleteValueResult(IEnumerable value, Func<object, IType> isTypeOf)
        {
            _value = value;
            IsTypeOf = isTypeOf;
        }

        public Func<object, IType> IsTypeOf { get; }

        public object? Value => _value;

        public ValueTask<object> CompleteValueAsync(
            IResolverContext context)
        {
            return CompleteValueAsync(_value, context.Field.Type, context.Path, context);
        }

        private ValueTask<object> CompleteValueAsync(
            object? value,
            IType fieldType,
            NodePath path,
            IResolverContext context)
        {
            if (value is IResolverResult)
            {
                throw new CompleteValueException($"Cannot complete value for field '{context.FieldName}':'{context.Field.Type}'. Resolving {nameof(IResolverResult)} value is not supported.",
                    path,
                    context.Selection);
            }


            if (fieldType is NonNull nonNull)
                return CompleteNonNullValueAsync(value, nonNull, path, context);

            if (value == null)
                return default;

            if (fieldType is List list)
                return CompleteListValueAsync(value, list, path, context);

            return fieldType switch
            {
                ScalarType scalarType => CompleteScalarType(value, scalarType, context),
                EnumType enumType => CompleteEnumType(value, enumType, context),
                ObjectType objectType => CompleteObjectValueAsync(value, objectType, path, context),

                InterfaceType interfaceType => CompleteInterfaceValueAsync(value, interfaceType, path, context),
                UnionType unionType => CompleteUnionValueAsync(value, unionType, path, context),
                _ => throw new CompleteValueException(
                    $"Cannot complete value for field {context.FieldName}. Cannot complete value of type {fieldType}.",
                    path,
                    context.Selection)
            };
        }

        private ValueTask<object> CompleteEnumType(object value, EnumType enumType, IResolverContext context)
        {
            //todo: use similar pattern to scalars
            return new ValueTask<object>(enumType.Serialize(value));
        }

        private ValueTask<object> CompleteScalarType(object value, ScalarType scalarType, IResolverContext context)
        {
            var converter = context.ExecutionContext.Schema.GetValueConverter(scalarType.Name);
            return new ValueTask<object>(converter.Serialize(value));
        }

        private async ValueTask<object> CompleteUnionValueAsync(
            object value,
            UnionType unionType,
            NodePath path,
            IResolverContext context)
        {
            var actualType = IsTypeOf(value) as ObjectType;

            if (actualType == null)
                throw new CompleteValueException(
                    $"Cannot complete value for field '{context.FieldName}':'{unionType}'. " +
                    "ActualType is required for union values.",
                    path,
                    context.Selection);

            if (!unionType.IsPossible(actualType))
                throw new CompleteValueException(
                    $"Cannot complete value for field '{context.FieldName}':'{unionType}'. " +
                    $"ActualType '{actualType}' is not possible for '{unionType}'",
                    path,
                    context.Selection);

            var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context.ExecutionContext,
                subSelectionSet,
                actualType,
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
            var actualType = IsTypeOf(value) as ObjectType;

            if (actualType == null)
                throw new CompleteValueException(
                    $"Cannot complete value for field '{context.FieldName}':'{interfaceType}'. " +
                    "ActualType is required for interface values.",
                    path,
                    context.Selection);

            if (!actualType.Implements(interfaceType))
                throw new CompleteValueException(
                    $"Cannot complete value for field '{context.FieldName}':'{interfaceType}'. " +
                    $"ActualType '{actualType}' does not implement interface '{interfaceType}'",
                    path,
                    context.Selection);

            var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context.ExecutionContext,
                subSelectionSet,
                actualType,
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
                    path,
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
                    $"Cannot complete value for list field '{context.FieldName}':'{list}'. " +
                    "Resolved value is not a collection",
                    path,
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