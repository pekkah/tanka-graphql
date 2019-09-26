using System;
using System.Linq;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Tools
{
    public static class MergeTool
    {
        public static void Schemas(SchemaBuilder target, params ISchema[] schemas)
        {
            foreach (var right in schemas) Schema(target, right);
        }

        public static void Schema(SchemaBuilder target, ISchema right)
        {
            foreach (var rightType in right.QueryTypes<ComplexType>())
                MergeComplexType(right, target, rightType);

            foreach (var rightType in right.QueryTypes<InputObjectType>()) MergeInputType(right, target, rightType);

            foreach (var rightType in right.QueryTypes<ScalarType>()) MergeScalarType(target, rightType);

            foreach (var directiveType in right.QueryDirectiveTypes())
            {
                target.TryGetDirective(directiveType.Name, out var leftDirective);

                if (leftDirective != null)
                    continue;

                target.Include(directiveType);
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
            if (builder.TryGetType<ComplexType>(rightType.Name, out var leftType))
            {
                var rightTypeFields = right.GetFields(rightType.Name);

                foreach (var rightTypeField in rightTypeFields)
                    builder.Connections(connect =>
                    {
                        if (connect.TryGetField(leftType, rightTypeField.Key, out _))
                            return;

                        connect.Include(leftType, new[] {rightTypeField});

                        var resolver = right.GetResolver(rightType.Name, rightTypeField.Key);

                        if (resolver != null)
                            connect.GetOrAddResolver(leftType, rightTypeField.Key)
                                .Run(resolver);

                        var subscriber = right.GetSubscriber(rightType.Name, rightTypeField.Key);

                        if (subscriber != null)
                            connect.GetOrAddSubscriber(leftType, rightTypeField.Key)
                                .Run(subscriber);
                    });
            }
            else
            {
                builder
                    .Include(rightType)
                    .Connections(connect =>
                    {
                        var fields = right.GetFields(rightType.Name).ToList();
                        connect.Include(rightType, fields);

                        foreach (var rightTypeField in fields)
                        {
                            var resolver = right.GetResolver(rightType.Name, rightTypeField.Key);

                            if (resolver != null)
                                connect.GetOrAddResolver(rightType, rightTypeField.Key)
                                    .Run(resolver);

                            var subscriber = right.GetSubscriber(rightType.Name, rightTypeField.Key);

                            if (subscriber != null)
                                connect.GetOrAddSubscriber(rightType, rightTypeField.Key)
                                    .Run(subscriber);
                        }
                    });
            }
        }
    }
}