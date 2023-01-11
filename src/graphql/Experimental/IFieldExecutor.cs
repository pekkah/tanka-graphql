using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Experimental;

public interface IFieldExecutor
{
    static IFieldExecutor Default = new FieldExecutor();

    Task<object?> Execute(
        QueryContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path);
}

public class FieldExecutor : IFieldExecutor
{
    public async Task<object?> Execute(
        QueryContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path)
    {
        var schema = context.Schema;
        var fieldSelection = fields.First();
        var fieldName = fieldSelection.Name;
        path.Append(fieldName);

        // __typename hack
        //if (fieldName == "__typename") return objectDefinition.Name.Value;

        var fieldType = schema
            .GetField(objectDefinition.Name, fieldName)?
            .Type;

        if (fieldType == null)
            throw new QueryException(
                $"Object '{objectDefinition.Name}' does not have field '{fieldName}'")
            {
                Path = path
            };

        var field = schema.GetField(objectDefinition.Name, fieldName);
        object? completedValue = null;

        if (field is null)
            return null;

        var argumentValues = ArgumentCoercion.CoerceArgumentValues(
            schema,
            objectDefinition,
            fieldSelection,
            context.CoercedVariableValues);

        try
        {
            var resolver = schema.GetResolver(objectDefinition.Name, fieldName);

            if (resolver == null)
                throw new QueryException(
                    $"Could not get resolver for {objectDefinition.Name}.{fieldName}")
                {
                    Path = path
                };

            var resolverContext = new ResolverContext
            {
                Arguments = argumentValues,
                Field = field,
                Fields = fields,
                ObjectDefinition = objectDefinition,
                ObjectValue = objectValue,
                Path = path,
                Selection = fieldSelection,
                QueryContext = context
            };


            var resolvedValue = await resolver(resolverContext);
            return await CompleteValueAsync(resolvedValue, fieldType, resolverContext, path);
        }
        catch (Exception e)
        {
            return e.Handle(
                context,
                objectDefinition,
                fieldName,
                fieldType,
                fieldSelection,
                completedValue,
                path);
        }
    }


    private ValueTask<object?> CompleteValueAsync(
        object? value,
        TypeBase fieldType,
        ResolverContext context,
        NodePath path)
    {
        if (fieldType is NonNullType nonNullType)
            return CompleteNonNullTypeValueAsync(value, nonNullType, path, context);

        if (value == null)
            return default;

        if (fieldType is ListType list)
            return CompleteListValueAsync(value, list, path, context);

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
        return new ValueTask<object?>(new EnumConverter(enumType).Serialize(value));
    }

    private ValueTask<object?> CompleteScalarType(object? value, ScalarDefinition scalarType, ResolverContext context)
    {
        var converter = context.QueryContext.Schema.GetRequiredValueConverter(scalarType.Name);
        return new ValueTask<object?>(converter.Serialize(value));
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

        var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
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

        var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
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
        var subSelectionSet = SelectionSets.MergeSelectionSets(context.Fields);
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