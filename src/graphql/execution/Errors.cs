using System;
using System.Collections.Generic;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Errors
    {
        public static object HandleFieldError(
            ICollection<Exception> errors,
            ObjectType objectType,
            string fieldName,
            IGraphQLType fieldType,
            GraphQLFieldSelection fieldSelection,
            object completedValue,
            Exception exception)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));

            var error = new FieldErrorException(
                $"{objectType.Name}.{fieldName} has an error", objectType, fieldName, fieldType, fieldSelection,
                completedValue, exception);

            if (fieldType is NonNull)
                throw error;

            errors.Add(exception);
            return completedValue;
        }
    }
}