using System.Collections;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.ValueResolution;

public interface IValueCompletionFeature
{
    ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path);
}

public class ValueCompletionFeature : IValueCompletionFeature
{
    public async ValueTask CompleteValueAsync(
        ResolverContext context,
        TypeBase fieldType,
        NodePath path)
    {
        context.CompletedValue = await CompleteValueAsync(context.ResolvedValue, fieldType, context, path);
    }

    public ValueTask<object?> CompleteValueAsync(
        object? value,
        TypeBase fieldType,
        ResolverContext context,
        NodePath path)
    {
        if (fieldType is NonNullType nonNullType)
            return CompleteNonNullTypeValueAsync(value, nonNullType, path, context);

        if (value == null)
            return default;

        if (fieldType is ListType list) return CompleteListValueAsync(value, list, path, context);

        if (fieldType is not NamedType namedType)
            throw new InvalidOperationException("FieldType is not NamedType");

        var typeDefinition = context.QueryContext.Schema.GetRequiredNamedType<TypeDefinition>(namedType.Name);
        return typeDefinition switch
        {
            ScalarDefinition scalarType => CompleteScalarType(value, scalarType, context),
            EnumDefinition enumType => CompleteEnumType(value, enumType, context),
            ObjectDefinition objectDefinition => CompleteObjectValueAsync(value, objectDefinition, path, context),

            InterfaceDefinition interfaceType => CompleteInterfaceValueAsync(value, interfaceType, path, context),
            UnionDefinition unionDefinition => CompleteUnionValueAsync(value, unionDefinition, path, context),
            _ => throw new FieldException(
                $"Cannot complete value for field {context.Field.Name}. Cannot complete value of type {Printer.Print(fieldType)}.")
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            }
        };
    }

    private ValueTask<object?> CompleteEnumType(object? value, EnumDefinition enumType, ResolverContext context)
    {
        //todo: use similar pattern to scalars
        return new(new EnumConverter(enumType).Serialize(value));
    }

    private ValueTask<object?> CompleteScalarType(object? value, ScalarDefinition scalarType, ResolverContext context)
    {
        var converter = context.QueryContext.Schema.GetRequiredValueConverter(scalarType.Name);
        return new(converter.Serialize(value));
    }

    private async ValueTask<object?> CompleteUnionValueAsync(
        object value,
        UnionDefinition unionDefinition,
        NodePath path,
        ResolverContext context)
    {
        var actualType = context.ResolveAbstractType?.Invoke(unionDefinition, value) as ObjectDefinition;

        if (actualType == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "ActualType is required for union values.")
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        if (!unionDefinition.HasMember(actualType.Name))
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                $"ActualType '{actualType.Name}' is not possible for '{unionDefinition.Name}'"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        var subSelectionSet = SelectionSetExtensions.MergeSelectionSets(context.Fields);
        var data = await context.QueryContext.ExecuteSelectionSet(
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
        ResolverContext context)
    {
        var actualType = context.ResolveAbstractType?.Invoke(interfaceType, value) as ObjectDefinition;

        if (actualType == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "ActualType is required for interface values."
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        if (!actualType.HasInterface(interfaceType.Name))
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                $"ActualType '{actualType.Name}' does not implement interface '{interfaceType.Name}'"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        var subSelectionSet = SelectionSetExtensions.MergeSelectionSets(context.Fields);
        var data = await context.QueryContext.ExecuteSelectionSet(
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
        ResolverContext context)
    {
        var subSelectionSet = SelectionSetExtensions.MergeSelectionSets(context.Fields);
        var data = await context.QueryContext.ExecuteSelectionSet(
            subSelectionSet,
            objectDefinition,
            value,
            path);

        return data;
    }

    private async ValueTask<object?> CompleteNonNullTypeValueAsync(
        object? value,
        NonNullType nonNullType,
        NodePath path,
        ResolverContext context)
    {
        var innerType = nonNullType.OfType;
        var completedResult = await CompleteValueAsync(value, innerType, context, path);

        if (completedResult == null)
            throw new FieldException(
                $"Cannot complete value for field '{Printer.Print(context.Field)}'. " +
                "Completed value would be null for non-null field"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        return completedResult;
    }

    private async ValueTask<object?> CompleteListValueAsync(
        object value,
        ListType list,
        NodePath path,
        ResolverContext context)
    {
        if (value is not IEnumerable values)
            throw new FieldException(
                $"Cannot complete value for list field '{context.Field.Name}':'{list}'. " +
                "Resolved value is not a collection"
            )
            {
                Path = context.Path,
                Field = context.Field,
                Selection = context.Selection,
                ObjectDefinition = context.ObjectDefinition
            };

        var innerType = list.OfType;
        var result = new List<object?>();
        var i = 0;
        foreach (var resultItem in values)
        {
            var itemPath = path.Fork().Append(i++);

            try
            {
                var completedResultItem = await CompleteValueAsync(
                    resultItem,
                    innerType,
                    context,
                    itemPath);

                result.Add(completedResultItem);
            }
            catch (Exception e)
            {
                if (innerType is NonNullType) throw;

                context.QueryContext.ErrorCollector.Add(e);
                result.Add(null);
            }
        }

        return result;
    }
}