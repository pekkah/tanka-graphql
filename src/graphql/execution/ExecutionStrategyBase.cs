using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.error;
using tanka.graphql.resolvers;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public abstract class ExecutionStrategyBase : IExecutionStrategy
    {
        public abstract Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(IExecutorContext context,
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            NodePath path);

        public async Task<object> ExecuteFieldAsync(
            IExecutorContext context,
            ObjectType objectType,
            object objectValue,
            List<GraphQLFieldSelection> fields,
            IType fieldType,
            IReadOnlyDictionary<string, object> coercedVariableValues, 
            NodePath path)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            var schema = context.Schema;
            var fieldSelection = fields.First();
            var fieldName = fieldSelection.Name.Value;
            var field = schema.GetField(objectType.Name, fieldName);
            object completedValue = null;

            var argumentValues = Arguments.CoerceArgumentValues(
                schema,
                objectType,
                fieldSelection,
                coercedVariableValues);

            try
            {
                var resolverContext =
                    new ResolverContext(
                        context.Schema,
                        objectType,
                        objectValue,
                        field,
                        fieldSelection,
                        argumentValues,
                        path,
                        context);

                var resolver = schema.GetResolver(objectType.Name, fieldName);

                if (resolver == null)
                    throw new GraphQLError($"Could not get resolver for {objectType.Name}.{fieldName}");

                resolver = context.Extensions.Resolver(resolverContext, resolver);
                var result = await resolver(resolverContext).ConfigureAwait(false);
                completedValue = await result.CompleteValueAsync(
                    context,
                    objectType,
                    field,
                    fieldType,
                    fieldSelection,
                    fields,
                    coercedVariableValues,
                    path).ConfigureAwait(false);

                return completedValue;
            }
            catch (Exception e)
            {
                return ExecutionErrors.HandleFieldError(
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

        protected async Task<object> ExecuteFieldGroupAsync(
            IExecutorContext context,
            ObjectType objectType,
            object objectValue,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            KeyValuePair<string, List<GraphQLFieldSelection>> fieldGroup, 
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
                throw new GraphQLError(
                    $"Object '{objectType.Name}' does not have field '{fieldName}'");

            var responseValue = await ExecuteFieldAsync(
                context,
                objectType,
                objectValue,
                fields,
                fieldType,
                coercedVariableValues,
                path).ConfigureAwait(false);

            return responseValue;
        }
    }
}