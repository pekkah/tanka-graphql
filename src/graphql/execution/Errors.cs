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
                throw new GraphQLError($"Field '{objectType}.{fieldName}:{fieldType}' is non-null field and cannot be resolved as null.",
                    new[] {fieldSelection}, 
                    locations: new []{ fieldSelection.Location},
                    originalError: error,
                    path: path);

            return completedValue;
        }
    }
}