using System;
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.type;

namespace fugu.graphql.tools
{
    public delegate IEnumerable<KeyValuePair<string, IField>> FieldConflictResolver(
        ComplexType left,
        ComplexType right,
        KeyValuePair<string, IField> conflict);


    public static class MergeTool
    {
        public static ISchema MergeSchemas(
            ISchema left,
            ISchema right,
            FieldConflictResolver fieldConflict)
        {
            var types = new Dictionary<string, IGraphQLType>();
            foreach (var leftType in left.QueryTypes<IGraphQLType>())
            {
                // this shouldn't be true but leaving for now
                if (string.IsNullOrEmpty(leftType.Name))
                    continue;

                types.Add(leftType.Name, leftType);
            }

            foreach (var rightType in right.QueryTypes<IGraphQLType>())
            {
                // this shouldn't be true but leaving for now
                if (string.IsNullOrEmpty(rightType.Name))
                    continue;

                // conflict
                if (types.ContainsKey(rightType.Name))
                {
                    var leftType = types[rightType.Name];

                    if (leftType is ObjectType leftObject && rightType is ObjectType rightObject)
                        types[rightType.Name] = Merge(leftObject, rightObject, fieldConflict);
                    else if (leftType is InterfaceType leftInterface && rightType is InterfaceType rightInterface)
                        types[rightType.Name] = Merge(leftInterface, rightInterface, fieldConflict);
                    else if (leftType is ScalarType)
                        continue;
                    else
                        throw new NotImplementedException();

                    continue;
                }

                types[rightType.Name] = rightType;
            }

            var queryType = types.SingleOrDefault(t => t.Key == left.Query.Name).Value as ObjectType;
            ObjectType mutationType = null;
            ObjectType subscriptionType = null;

            if (left.Mutation != null)
                mutationType = types.SingleOrDefault(t => t.Key == left.Mutation.Name).Value as ObjectType;

            if (left.Subscription != null)
                subscriptionType = types.SingleOrDefault(t => t.Key == left.Subscription.Name).Value as ObjectType;

            return new Schema(queryType, mutationType, subscriptionType, types.Values, left.QueryDirectives());
        }

        public static InterfaceType Merge(InterfaceType left, InterfaceType right,
            FieldConflictResolver conflictResolver)
        {
            var fields = MergeFields(left, right, conflictResolver);

            return new InterfaceType(
                left.Name,
                fields,
                left.Meta);
        }

        public static ObjectType Merge(ObjectType left, ObjectType right,
            FieldConflictResolver conflictResolver)
        {
            var interfaces = left.Interfaces
                .Concat(right.Interfaces)
                .Distinct();

            var fields = MergeFields(left, right, conflictResolver);

            return new ObjectType(
                left.Name,
                fields,
                left.Meta,
                interfaces);
        }

        public static IResolverMap Merge(params IResolverMap[] resolverMaps)
        {
            return new JoinedResolversMap(resolverMaps);
        }

        private static Fields MergeFields(ComplexType left, ComplexType right, FieldConflictResolver conflictResolver)
        {
            var fields = new Fields();
            foreach (var field in left.Fields) fields[field.Key] = field.Value;

            foreach (var field in right.Fields)
            {
                // conflict?
                if (fields.ContainsKey(field.Key))
                {
                    var resolvedFields = conflictResolver(left, right, field);
                    foreach (var resolvedField in resolvedFields) fields[resolvedField.Key] = resolvedField.Value;

                    continue;
                }

                fields[field.Key] = field.Value;
            }

            return fields;
        }
    }
}