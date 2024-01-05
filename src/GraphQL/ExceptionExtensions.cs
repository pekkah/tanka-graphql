using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public static class ExceptionExtensions
{
    public static object? Handle(
        this Exception exception,
        QueryContext context,
        ObjectDefinition objectDefinition,
        string fieldName,
        TypeBase fieldType,
        FieldSelection fieldSelection,
        object? completedValue,
        NodePath path)
    {
        if (exception is not FieldException)
            exception = new FieldException(exception.Message, exception)
            {
                ObjectDefinition = objectDefinition,
                Field = context.Schema.GetField(objectDefinition.Name, fieldName),
                Selection = fieldSelection,
                Path = path
            };

        if (fieldType is NonNullType)
            throw exception;

        context.AddError(exception);
        return completedValue;
    }

    public static void Handle(this Exception exception, ResolverContext context)
    {
        var objectDefinition = context.ObjectDefinition;
        var fieldSelection = context.Selection;
        var fieldType = context.Field?.Type;
        var path = context.Path;
        var fieldName = context.FieldName;

        if (exception is not FieldException)
            exception = new FieldException(exception.Message, exception)
            {
                ObjectDefinition = objectDefinition,
                Field = context.Schema.GetField(objectDefinition.Name, fieldName),
                Selection = fieldSelection,
                Path = path
            };

        if (fieldType is NonNullType)
            throw exception;

        context.QueryContext.AddError(exception);
    }
}