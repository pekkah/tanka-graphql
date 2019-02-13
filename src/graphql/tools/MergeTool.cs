using tanka.graphql.type;

namespace tanka.graphql.tools
{
    public static class MergeTool
    {
        public static ISchema MergeSchemas(
            ISchema left,
            ISchema right)
        {
            var builder = new SchemaBuilder(left);

            foreach (var rightType in right.QueryTypes<ComplexType>())
                if (builder.IsPredefinedType<ComplexType>(rightType.Name, out var leftType))
                {
                    var rightTypeFields = right.GetFields(rightType.Name);

                    foreach (var rightTypeField in rightTypeFields)
                        builder.Connections(connect =>
                        {
                            if (!connect.IsPredefinedField(leftType, rightTypeField.Key, out _))
                                connect.IncludeFields(leftType, new[] {rightTypeField});
                        });
                }
                else if (builder.IsPredefinedType<ScalarType>(rightType.Name, out var leftScalarType))
                {
                    // noop
                }
                else
                {
                    builder
                        .Include(rightType)
                        .Connections(connect => connect.IncludeFields(rightType, right.GetFields(rightType.Name)));
                }

            // todo: input objects

            return builder.Build();
        }
    }
}