using System;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Execution;

public static class FieldErrors
{
    public static object? Handle(
        IExecutorContext context,
        ObjectDefinition objectDefinition,
        string fieldName,
        TypeBase fieldType,
        FieldSelection fieldSelection,
        object? completedValue,
        Exception error,
        NodePath path)
    {
        if (error is not QueryExecutionException)
            error = new QueryExecutionException(
                "",
                error,
                path,
                fieldSelection);

        if (fieldType is NonNullType)
            throw error;

        context.AddError(error);
        return completedValue;
    }
}