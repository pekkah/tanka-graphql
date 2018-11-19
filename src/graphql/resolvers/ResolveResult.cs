using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.error;
using fugu.graphql.execution;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.resolvers
{
    public class ResolveResult : IResolveResult
    {
        public ResolveResult(object value)
        {
            Value = value;
        }

        public ResolveResult(ObjectType objectType, object value)
            : this(value)
        {
            ActualType = objectType;
        }

        public ResolveResult(IEnumerable values)
        {
            Value = values;
        }

        public ResolveResult(IEnumerable<IResolveResult> values)
        {
            Value = values;
        }

        public object Value { get; protected set; }

        public ObjectType ActualType { get; set; }

        public virtual async Task<object> CompleteValueAsync(
            IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IGraphQLType fieldType,
            GraphQLFieldSelection selection,
            List<GraphQLFieldSelection> fields,
            Dictionary<string, object> coercedVariableValues)
        {
            object completedValue = null;

            completedValue = await CompleteValueAsync(
                executorContext,
                objectType,
                field,
                fieldType,
                ActualType,
                selection,
                fields,
                Value,
                coercedVariableValues).ConfigureAwait(false);

            return completedValue;
        }

        public async Task<object> CompleteValueAsync(
            IExecutorContext executorContext,
            ObjectType objectType,
            IField field,
            IGraphQLType fieldType,
            ObjectType actualType,
            GraphQLFieldSelection selection,
            List<GraphQLFieldSelection> fields,
            object value,
            Dictionary<string, object> coercedVariableValues)
        {
            if (value is IResolveResult resolveResult)
                return await resolveResult.CompleteValueAsync(
                    executorContext,
                    objectType,
                    field,
                    fieldType,
                    selection,
                    fields,
                    coercedVariableValues).ConfigureAwait(false);

            if (fieldType is NamedTypeReference typeReference)
            {
                throw new InvalidOperationException(
                    $"NamedTypeReferences are not supported during execution. Please heal schema before execution.");

                /*var actualTypeName = typeReference.TypeName;
                var innerType = executorContext.Schema.GetNamedType(actualTypeName);

                if (innerType == null)
                    throw new GraphQLError(
                        $"Cannot complete value of '{selection.Name.Value}':'{nameof(NamedTypeReference)}' field. " +
                        $"Could not get named type '{actualTypeName}' from schema.");

                return await CompleteValueAsync(
                    executorContext,
                    objectType,
                    field,
                    innerType,
                    actualType,
                    selection,
                    fields,
                    value,
                    coercedVariableValues).ConfigureAwait(false);*/
            }

            if (fieldType is Lazy lazy)
                return await CompleteValueAsync(
                    executorContext,
                    objectType,
                    field,
                    lazy.WrappedType,
                    actualType,
                    selection,
                    fields,
                    value,
                    coercedVariableValues).ConfigureAwait(false);

            if (fieldType is NonNull nonNull)
            {
                var innerType = nonNull.WrappedType;
                var completedResult = await CompleteValueAsync(
                    executorContext,
                    objectType,
                    field,
                    innerType,
                    actualType,
                    selection,
                    fields,
                    value,
                    coercedVariableValues).ConfigureAwait(false);

                if (completedResult == null)
                    throw new NullValueForNonNullTypeException(
                        $"Cannot complete value on non-null field '{selection.Name.Value}':'{fieldType.Name}'. " +
                        $"Value is null", fieldType);

                return completedResult;
            }

            if (value == null)
                return null;


            if (fieldType is List listType)
            {
                if (!(value is IEnumerable values))
                    throw new GraphQLError(
                        $"Cannot complete value for list field '{selection.Name.Value}':'{fieldType.Name}'. " +
                        "Resolved value is not a collection");

                var innerType = listType.WrappedType;
                var result = new List<object>();
                foreach (var resultItem in values)
                {
                    var completedResultItem = await CompleteValueAsync(
                        executorContext,
                        objectType,
                        field,
                        innerType,
                        actualType,
                        selection,
                        fields,
                        resultItem,
                        coercedVariableValues).ConfigureAwait(false);

                    result.Add(completedResultItem);
                }

                return result;
            }

            if (fieldType is ScalarType scalarType) return scalarType.Serialize(value);

            if (fieldType is EnumType enumType) return enumType.Serialize(value);

            if (fieldType is ObjectType fieldObjectType)
            {
                var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
                var data = await SelectionSets.ExecuteSelectionSetAsync(
                    executorContext,
                    subSelectionSet,
                    fieldObjectType,
                    value,
                    coercedVariableValues).ConfigureAwait(false);

                return data;
            }

            // interfaces and unions require ActualType
            if (actualType == null)
                throw new GraphQLError(
                    $"Cannot complete value as interface or union. " +
                    $"Actual type not given from resolver. Use {nameof(Resolve.As)} with type parameter");

            if (fieldType is InterfaceType interfaceType)
            {
                if (!actualType.Implements(interfaceType))
                    throw new GraphQLError(
                        $"Cannot complete value as interface. " +
                        $"Actual type {actualType.Name} does not implement {interfaceType.Name}");

                var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
                var data = await SelectionSets.ExecuteSelectionSetAsync(
                    executorContext,
                    subSelectionSet,
                    actualType,
                    value,
                    coercedVariableValues).ConfigureAwait(false);

                return data;
            }

            if (fieldType is UnionType unionType)
            {
                if (!unionType.IsPossible(actualType))
                    throw new GraphQLError(
                        $"Cannot complete value as union. " +
                        $"Actual type {actualType.Name} is not possible for {unionType.Name}");

                var subSelectionSet = SelectionSets.MergeSelectionSets(fields);
                var data = await SelectionSets.ExecuteSelectionSetAsync(
                    executorContext,
                    subSelectionSet,
                    actualType,
                    value,
                    coercedVariableValues).ConfigureAwait(false);

                return data;
            }

            throw new GraphQLError($"Cannot complete value for field {fieldType.Name}. No handling for the type.");
        }
    }
}