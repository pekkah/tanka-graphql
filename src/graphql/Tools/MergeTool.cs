using System;
using System.Linq;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tools
{
    [Obsolete("Use SchemaBuilder.Merge")]
    public static class MergeTool
    {
        public static void Schemas(SchemaBuilder target, params ISchema[] schemas)
        {
            foreach (var right in schemas) Schema(target, right);
        }

        public static void Schema(SchemaBuilder target, ISchema right)
        {
            foreach (var rightType in right.QueryTypes<EnumType>())
                MergeEnumType(right, target, rightType);

            foreach (var rightType in right.QueryTypes<InputObjectType>()) 
                MergeInputType(right, target, rightType);

            foreach (var rightType in right.QueryTypes<ScalarType>())
                MergeScalarType(target, rightType);

            foreach (var directiveType in right.QueryDirectiveTypes())
            {
                target.TryGetDirective(directiveType.Name, out var leftDirective);

                if (leftDirective != null)
                    continue;

                target.Include(directiveType);
            }

            // merge complex types
            foreach (var rightType in right.QueryTypes<ComplexType>())
                MergeComplexType(right, target, rightType);

            // merge complex type field
            foreach (var rightType in right.QueryTypes<ComplexType>())
                MergeComplexTypeFields(right, target, rightType);
        }

        private static void MergeEnumType(ISchema right, SchemaBuilder builder, EnumType rightType)
        {
            // merge values if enum exists in both
            if (builder.TryGetType<EnumType>(rightType.Name, out var leftEnumType))
            {
                var leftValues = leftEnumType.Values.ToList();
                var rightValues = rightType.Values;

                // combine values
                foreach (var rightValue in rightValues)
                {
                    // if key is same then the values are same
                    if (leftValues.Any(v => v.Key == rightValue.Key))
                        continue;

                    //todo: should we merge directives?

                    leftValues.Add(rightValue);
                }

                builder.Remove(leftEnumType);
                builder.Enum(
                    leftEnumType.Name,
                    out _,
                    rightType.Description,
                    values => leftValues.ForEach(v => values.Value(
                        v.Value.Value, 
                        v.Value.Description, 
                        v.Value.Directives,
                        v.Value.DeprecationReason)),
                    rightType.Directives);
            }
            else
            {
                // include whole enum
                builder.Include(rightType);
            }
        }

        private static void MergeScalarType(SchemaBuilder builder, ScalarType rightType)
        {
            if (!builder.TryGetType<ScalarType>(rightType.Name, out _)) builder.Include(rightType);
        }

        private static void MergeInputType(ISchema right, SchemaBuilder builder, InputObjectType rightType)
        {
            if (builder.TryGetType<InputObjectType>(rightType.Name, out var leftType))
            {
                var rightTypeFields = right.GetInputFields(rightType.Name);

                foreach (var rightTypeField in rightTypeFields)
                    builder.Connections(connect =>
                    {
                        if (connect.TryGetInputField(leftType, rightTypeField.Key, out _))
                            return;

                        connect.Include(leftType, new[] {rightTypeField});
                    });
            }
            else
            {
                builder
                    .Include(rightType)
                    .Connections(connect =>
                    {
                        var fields = right.GetInputFields(rightType.Name).ToList();
                        connect.Include(rightType, fields);
                    });
            }
        }

        private static void MergeComplexType(ISchema right, SchemaBuilder builder, ComplexType rightType)
        {
            // complex type from right is missing so include it
            if (!builder.TryGetType<ComplexType>(rightType.Name, out _))
            {
                builder.Include(rightType);
            }
        }

        private static void MergeComplexTypeFields(ISchema right, SchemaBuilder builder, ComplexType rightType)
        {
            if (builder.TryGetType<ComplexType>(rightType.Name, out var leftType))
            {
                var rightTypeFields = right.GetFields(rightType.Name);

                foreach (var rightTypeField in rightTypeFields)
                    builder.Connections(connect =>
                    {
                        // if field already exists skip it
                        if (connect.TryGetField(leftType, rightTypeField.Key, out _))
                            return;

                        // include field
                        connect.Include(leftType, rightTypeField);

                        // include resolver
                        var resolver = right.GetResolver(rightType.Name, rightTypeField.Key);

                        if (resolver != null)
                            connect.GetOrAddResolver(leftType, rightTypeField.Key)
                                .Run(resolver);

                        // include subscriber
                        var subscriber = right.GetSubscriber(rightType.Name, rightTypeField.Key);

                        if (subscriber != null)
                            connect.GetOrAddSubscriber(leftType, rightTypeField.Key)
                                .Run(subscriber);
                    });
            }
            else
            {
                throw new SchemaBuilderException(
                    rightType.Name,
                    $"Cannot merge fields of {rightType}. Type is now known by builder. " +
                    $"Call MergeComplexType first.");
            }
        }
    }
}