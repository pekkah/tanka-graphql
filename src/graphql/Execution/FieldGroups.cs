using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public static class FieldGroups
    {
        public static async Task<object> ExecuteFieldAsync(
            IExecutorContext context,
            ObjectType objectType,
            object objectValue,
            IReadOnlyCollection<GraphQLFieldSelection> fields,
            IType fieldType,
            NodePath path)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

            var schema = context.Schema;
            var fieldSelection = fields.First();
            var fieldName = fieldSelection.Name.Value;
            var field = schema.GetField(objectType.Name, fieldName);
            object completedValue = null;

            var argumentValues = Arguments.CoerceArgumentValues(
                schema,
                objectType,
                fieldSelection,
                context.CoercedVariableValues);

            try
            {
                var resolver = schema.GetResolver(objectType.Name, fieldName);

                if (resolver == null)
                    throw new QueryExecutionException(
                        $"Could not get resolver for {objectType.Name}.{fieldName}",
                        path);

                var resolverContext =
                    new ResolverContext(
                        objectType,
                        objectValue,
                        field,
                        fieldSelection,
                        fields,
                        argumentValues,
                        path,
                        context);

                var result = context.ExtensionsRunner.Resolve(resolver, resolverContext);
                completedValue = await result.CompleteValueAsync(resolverContext);
                return completedValue;
            }
            catch (Exception e)
            {
                return FieldErrors.Handle(
                    context,
                    objectType,
                    fieldName,
                    fieldType,
                    fieldSelection,
                    completedValue,
                    e,
                    path);
            }
        }

        public static async Task<object> ExecuteFieldGroupAsync(
            IExecutorContext context,
            ObjectType objectType,
            object objectValue,
            KeyValuePair<string, IReadOnlyCollection<GraphQLFieldSelection>> fieldGroup, 
            NodePath path)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            var schema = context.Schema;
            var fields = fieldGroup.Value;
            var fieldName = fields.First().Name.Value;
            path.Append(fieldName);

            // __typename hack
            if (fieldName == "__typename")
            {
                return objectType.Name;
            }

            var fieldType = schema
                .GetField(objectType.Name, fieldName)?
                .Type;

            if (fieldType == null)
                throw new QueryExecutionException(
                    $"Object '{objectType.Name}' does not have field '{fieldName}'",
                    path);

            var responseValue = await ExecuteFieldAsync(
                context,
                objectType,
                objectValue,
                fields,
                fieldType,
                path).ConfigureAwait(false);

            return responseValue;
        }
    }
}