using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.tools
{
    [Obsolete]
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

    [Obsolete]
    public delegate IField FieldConflictResolver(
        ConflictingField left,
        ConflictingField right);


    public static class MergeTool
    {
        public static ISchema MergeSchemas(
            ISchema left,
            ISchema right,
            FieldConflictResolver fieldConflict = null)
        {
            var builder = new SchemaBuilder(left);

            foreach (var rightType in right.QueryTypes<ComplexType>())
            {
                if (builder.IsPredefinedType<ComplexType>(rightType.Name, out var leftType))
                {
                    var rightTypeFields = right.GetFields(rightType.Name);

                    foreach (var rightTypeField in rightTypeFields)
                    {
                        if (!builder.IsPredefinedField(leftType, rightTypeField.Key, out _))
                        {
                            builder.IncludeFields(leftType, new[] {rightTypeField});
                        }
                    }
                }
                else if (builder.IsPredefinedType<ScalarType>(rightType.Name, out var leftScalarType))
                {
                    // noop
                }
                else
                {
                    builder
                        .Include(rightType)
                        .IncludeFields(rightType, right.GetFields(rightType.Name));
                }
            }

            return builder.Build();
        }
    }
}