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
    public abstract class ExecutionContextBase : IExecutionContext
    {
        protected ExecutionContextBase(ISchema schema, GraphQLDocument document)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public ISchema Schema { get; }

        public GraphQLDocument Document { get; }

        public List<Exception> FieldErrors { get; } = new List<Exception>();

        public abstract Task<IDictionary<string, object>> ExecuteGroupedFieldSetAsync(
            Dictionary<string, List<GraphQLFieldSelection>> groupedFieldSet,
            ObjectType objectType, object objectValue,
            Dictionary<string, object> coercedVariableValues);

        public async Task<object> ExecuteFieldAsync(
            ObjectType objectType,
            object objectValue,
            List<GraphQLFieldSelection> fields,
            IGraphQLType fieldType,
            Dictionary<string, object> coercedVariableValues)
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
                    this,
                    objectType,
                    field,
                    fieldType,
                    fieldSelection,
                    fields,
                    coercedVariableValues).ConfigureAwait(false);

                return completedValue;
            }
            catch (Exception e)
            {
                return Errors.HandleFieldError(
                    FieldErrors,
                    objectType,
                    fieldName,
                    fieldType,
                    fieldSelection,
                    completedValue,
                    e);
            }
        }

        protected async Task<object> ExecuteFieldGroupAsync(
            ObjectType objectType,
            object objectValue,
            Dictionary<string, object> coercedVariableValues,
            KeyValuePair<string, List<GraphQLFieldSelection>> fieldGroup)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            var fields = fieldGroup.Value;
            var fieldName = fields.First().Name.Value;

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
                objectType,
                objectValue,
                fields,
                fieldType,
                coercedVariableValues).ConfigureAwait(false);
            return responseValue;
        }
    }
}