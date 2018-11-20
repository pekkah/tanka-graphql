using fugu.graphql.error;
using fugu.graphql.resolvers;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Errors
    {
        public static object HandleFieldError(
            IExecutorContext context,
            ObjectType objectType,
            string fieldName,
            IGraphQLType fieldType,
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