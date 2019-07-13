using System;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public static class FieldErrors
    {
        public static object Handle(
            IExecutorContext context,
            ObjectType objectType,
            string fieldName,
            IType fieldType,
            GraphQLFieldSelection fieldSelection,
            object completedValue,
            Exception error,
            NodePath path)
        {
            if (fieldType is NonNull)
                throw error;

            context.AddError(error);
            return completedValue;
        }
    }
}