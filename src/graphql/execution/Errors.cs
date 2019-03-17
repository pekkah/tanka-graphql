using tanka.graphql.resolvers;
using tanka.graphql.type;
using GraphQLParser.AST;
using tanka.graphql.error;

namespace tanka.graphql.execution
{
    public static class Errors
    {
        public static object HandleFieldError(
            IExecutorContext context,
            ObjectType objectType,
            string fieldName,
            IType fieldType,
            GraphQLFieldSelection fieldSelection,
            object completedValue,
            GraphQLError error,
            NodePath path)
        {
            if (!(error is CompleteValueException))
            {
                context.AddError(error);
            }

            if (fieldType is NonNull)
                throw error;

            return completedValue;
        }
    }
}