using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.error;
using fugu.graphql.resolvers;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public abstract class ExecutionStrategyBase : IExecutionStrategy
    {
        public abstract Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(IExecutorContext context,
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            Dictionary<string, object> coercedVariableValues, NodePath path);

        public async Task<object> ExecuteFieldAsync(IExecutorContext context,
            ObjectType objectType,
            object objectValue,
            List<GraphQLFieldSelection> fields,
            IGraphQLType fieldType,
            Dictionary<string, object> coercedVariableValues, NodePath path)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            var fieldSelection = fields.First();
            var fieldName = fieldSelection.Name.Value;
            var field = objectType.GetField(fieldName);
            object completedValue = null;

            var argumentValues = Arguments.CoerceArgumentValues(
                objectType,
                fieldSelection,
                coercedVariableValues);

            try
            {
                var resolverContext =
                    new ResolverContext(
                        objectType,
                        objectValue,
                        field,
                        fieldSelection,
                        argumentValues);

                var resolver = field.Resolve;

                if (resolver == null)
                    throw new GraphQLError(
                        $"Could not get resolver for {objectType.Name}.{fieldName}");

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
            catch (GraphQLError e)
            {
                return Errors.HandleFieldError(
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

        protected async Task<object> ExecuteFieldGroupAsync(IExecutorContext context,
            ObjectType objectType,
            object objectValue,
            Dictionary<string, object> coercedVariableValues,
            KeyValuePair<string, List<GraphQLFieldSelection>> fieldGroup, 
            NodePath path)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            var fields = fieldGroup.Value;
            var fieldName = fields.First().Name.Value;
            path.Append(fieldName);

            // __typename hack
            if (fieldName == "__typename")
            {
                return objectType.Unwrap().Name;
            }

            var fieldType = objectType
                .GetField(fieldName)?
                .Type;

            if (fieldType == null)
                throw new GraphQLError(
                    $"Object '{objectType.Name}' does not have field '{fieldName}'");

            object responseValue = null;
            responseValue = await ExecuteFieldAsync(
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