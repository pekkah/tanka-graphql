using System;
using tanka.graphql.resolvers;
using tanka.graphql.type;
using GraphQLParser.AST;
using tanka.graphql.error;

namespace tanka.graphql.execution
{
    public static class ExecutionErrors
    {
        public static object HandleFieldError(
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