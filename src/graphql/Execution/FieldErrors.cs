using System;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public static class FieldErrors
    {
        public static object Handle(
            IExecutorContext context,
            ObjectType objectType,
            string fieldName,
            IType fieldType,
            FieldSelection fieldSelection,
            object? completedValue,
            Exception error,
            NodePath path)
        {
            if (!(error is QueryExecutionException))
            {
                error = new QueryExecutionException(
                    "",
                    error,
                    path,
                    fieldSelection);
            }

            if (fieldType is NonNull)
                throw error;

            context.AddError(error);
            return completedValue;
        }
    }
}