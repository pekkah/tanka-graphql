using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.tools
{
    public class ConflictingField
    {
        public ConflictingField(ISchema schema, ComplexType type, string fieldName, IField field)
        {
            Schema = schema;
            Type = type;
            FieldName = fieldName;
            Field = field;
        }

        public ISchema Schema { get; }
        public ComplexType Type { get; }
        public string FieldName { get; }
        public IField Field { get; }
    }

    public delegate IField FieldConflictResolver(
        ConflictingField left,
        ConflictingField right);


    public static class MergeTool
    {
        public static ISchema MergeSchemas(
            ISchema left,
            ISchema right,
            FieldConflictResolver fieldConflict)
        {
            if (!left.IsInitialized || !right.IsInitialized)
                throw new InvalidOperationException($"Schemas must be initialized before merging");

            var types = new List<INamedType>();

            foreach (var leftType in left.QueryTypes<INamedType>())
            {
                var rightType = right.GetNamedType(leftType.Name);

                // type does not exists in right
                if (rightType == null)
                {
                    types.Add(leftType);
                    continue;
                }

                // requires merging

                // invalid types?
                if (leftType.GetType() != rightType.GetType())
                    throw new InvalidOperationException($"Cannot merge {rightType} to {leftType}. Types do not match.");

                // merge types
                if (leftType is InterfaceType leftInterface && rightType is InterfaceType rightInterface)
                {
                    var mergedType = Merge(leftInterface, rightInterface, fieldConflict);
                    types.Add(mergedType);
                }
                else if (leftType is ObjectType leftObject && rightType is ObjectType rightObject)
                {
                    var mergedType = Merge(leftObject, rightObject, fieldConflict);
                    types.Add(mergedType);
                }
                else
                {
                    if (leftType is ScalarType)
                        types.Add(leftType);
                }
            }

            foreach (var rightType in right.QueryTypes<INamedType>())
            {
                if (types.Any(t => t.Name == rightType.Name))
                    continue;

                types.Add(rightType);
            }

            return Schema.Initialize(
                types.Single(t => t.Name == left.Query.Name) as ObjectType,
                left.Mutation != null ? types.SingleOrDefault(t => t.Name == left.Mutation.Name) as ObjectType : null,
                left.Subscription != null
                    ? types.SingleOrDefault(t => t.Name == left.Subscription.Name) as ObjectType
                    : null);
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

        private static Fields MergeFields(ComplexType left, ComplexType right, FieldConflictResolver conflictResolver)
        {
            var fields = new Fields();
            foreach (var field in left.Fields)
                fields[field.Key] = field.Value;

            foreach (var field in right.Fields)
            {
                // conflict?
                if (fields.TryGetValue(field.Key, out var leftField))
                {
                    var resolvedField = conflictResolver(
                        new ConflictingField(null, left, field.Key, leftField),
                        new ConflictingField(null, right, field.Key, field.Value));

                    fields[field.Key] = resolvedField;

                    continue;
                }

                fields[field.Key] = field.Value;
            }

            return fields;
        }
    }
}