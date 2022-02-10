using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.ValueResolution
{
    public class CompleteValueResult : IResolverResult
    {
        private readonly object? _value;

        public CompleteValueResult(object? value, TypeDefinition actualType)
        {
            _value = value;
            IsTypeOf = (_,_) => actualType;
        }

        public CompleteValueResult(IEnumerable? value, Func<IResolverContext, object?, TypeDefinition> isTypeOf)
        {
            _value = value;
            IsTypeOf = isTypeOf;
        }

        public CompleteValueResult(object? value)
        {
            _value = value;
            IsTypeOf = (_, _) =>
                throw new InvalidOperationException("Abstract type value was resolved without actual type or isTypeOf being provided.");
        }

        public Func<IResolverContext, object?, TypeDefinition> IsTypeOf { get; }

        public object? Value => _value;

        public ValueTask<object?> CompleteValueAsync(
            IResolverContext context)
        {
            return CompleteValueAsync(_value, context.Field.Type, context.Path, context);
        }

        private ValueTask<object?> CompleteValueAsync(
            object? value,
            TypeBase fieldType,
            NodePath path,
            IResolverContext context)
        {
            if (value is IResolverResult)
            {
                throw new CompleteValueException($"Cannot complete value for field '{Printer.Print(context.Field)}'. Resolving {nameof(IResolverResult)} value is not supported.",
                    path,
                    context.Selection);
            }

            if (fieldType is NonNullType NonNullType)
                return CompleteNonNullTypeValueAsync(value, NonNullType, path, context);

            if (value == null)
                return default;

            if (fieldType is ListType list)
                return CompleteListValueAsync(value, list, path, context);

            if (fieldType is not NamedType namedType)
                throw new InvalidOperationException("FieldType is not NamedType");

            var typeDefinition = context.ExecutionContext.Schema.GetRequiredNamedType<TypeDefinition>(namedType.Name);
            return  typeDefinition switch
            {
                ScalarDefinition scalarType => CompleteScalarType(value, scalarType, context),
                EnumDefinition enumType => CompleteEnumType(value, enumType, context),
                ObjectDefinition objectDefinition => CompleteObjectValueAsync(value, objectDefinition, path, context),

                InterfaceDefinition interfaceType => CompleteInterfaceValueAsync(value, interfaceType, path, context),
                UnionDefinition unionDefinition => CompleteUnionValueAsync(value, unionDefinition, path, context),
                _ => throw new CompleteValueException(
                    $"Cannot complete value for field {context.FieldName}. Cannot complete value of type {Printer.Print(fieldType)}.",
                    path,
                    context.Selection)
            };
        }

        private ValueTask<object?> CompleteEnumType(object? value, EnumDefinition enumType, IResolverContext context)
        {
            //todo: use similar pattern to scalars
            return new ValueTask<object?>(new EnumConverter(enumType).Serialize(value));
        }

        private ValueTask<object?> CompleteScalarType(object? value, ScalarDefinition scalarType, IResolverContext context)
        {
            var converter = context.ExecutionContext.Schema.GetRequiredValueConverter(scalarType.Name);
            return new ValueTask<object?>(converter.Serialize(value));
        }

        private async ValueTask<object?> CompleteUnionValueAsync(
            object value,
            UnionDefinition unionDefinition,
            NodePath path,
            IResolverContext context)
        {
            var actualType = IsTypeOf(context, value) as ObjectDefinition;

            if (actualType == null)
                throw new CompleteValueException(
                    $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                    "ActualType is required for union values.",
                    path,
                    context.Selection);

            if (!unionDefinition.HasMember(actualType.Name))
                throw new CompleteValueException(
                    $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                    $"ActualType '{actualType.Name}' is not possible for '{unionDefinition.Name}'",
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

        private async ValueTask<object?> CompleteInterfaceValueAsync(
            object value,
            InterfaceDefinition interfaceType,
            NodePath path,
            IResolverContext context)
        {
            var actualType = IsTypeOf(context, value) as ObjectDefinition;

            if (actualType == null)
                throw new CompleteValueException(
                    $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                    "ActualType is required for interface values.",
                    path,
                    context.Selection);

            if (!actualType.HasInterface(interfaceType.Name))
                throw new CompleteValueException(
                    $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                    $"ActualType '{actualType.Name}' does not implement interface '{interfaceType.Name}'",
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

        private static async ValueTask<object?> CompleteObjectValueAsync(
            object value,
            ObjectDefinition objectDefinition,
            NodePath path,
            IResolverContext context)
        {
            var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
            var data = await SelectionSets.ExecuteSelectionSetAsync(
                context.ExecutionContext,
                subSelectionSet,
                objectDefinition,
                value,
                path);

            return data;
        }

        private async ValueTask<object?> CompleteNonNullTypeValueAsync(
            object? value,
            NonNullType NonNullType,
            NodePath path,
            IResolverContext context)
        {
            var innerType = NonNullType.OfType;
            var completedResult = await CompleteValueAsync(value, innerType, path, context);

            if (completedResult == null)
                throw new NullValueForNonNullTypeException(
                    context.ObjectDefinition.Name,
                    context.FieldName,
                    path,
                    context.Selection);

            return completedResult;
        }

        private async ValueTask<object?> CompleteListValueAsync(
            object value,
            ListType list,
            NodePath path,
            IResolverContext context)
        {
            if (value is not IEnumerable values)
                throw new CompleteValueException(
                    $"Cannot complete value for list field '{context.FieldName}':'{list}'. " +
                    "Resolved value is not a collection",
                    path,
                    context.Selection);

            var innerType = list.OfType;
            var result = new List<object?>();
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
                    if (innerType is NonNullType) throw;

                    context.ExecutionContext.AddError(e);
                    result.Add(null);
                }
            }

            return result;
        }
    }
}