using System.Linq;
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
                if (builder.TryGetType<ComplexType>(rightType.Name, out var leftType))
                {
                    var rightTypeFields = right.GetFields(rightType.Name);

                    foreach (var rightTypeField in rightTypeFields)
                    {
                        builder.Connections(connect =>
                        {
                            if (connect.TryGetField(leftType, rightTypeField.Key, out _)) 
                                return;
                            
                            connect.IncludeFields(leftType, new[] {rightTypeField});

                            var resolver = right.GetResolver(rightType.Name, rightTypeField.Key);

                            if (resolver != null)
                            {
                                connect.GetOrAddResolver(leftType, rightTypeField.Key)
                                    .Run(resolver);
                            }

                            var subscriber = right.GetSubscriber(rightType.Name, rightTypeField.Key);

                            if (subscriber != null)
                            {
                                connect.GetOrAddSubscriber(leftType, rightTypeField.Key);
                            }
                        });
                    }
                }
                else
                {
                    builder
                        .Include(rightType)
                        .Connections(connect =>
                        {
                            var fields = right.GetFields(rightType.Name).ToList();
                            connect.IncludeFields(rightType, fields);

                            foreach (var rightTypeField in fields)
                            {
                                var resolver = right.GetResolver(rightType.Name, rightTypeField.Key);

                                if (resolver != null)
                                {
                                    connect.GetOrAddResolver(rightType, rightTypeField.Key)
                                        .Run(resolver);
                                }

                                var subscriber = right.GetSubscriber(rightType.Name, rightTypeField.Key);

                                if (subscriber != null)
                                {
                                    connect.GetOrAddSubscriber(rightType, rightTypeField.Key);
                                }
                            }
                        });
                }


            foreach (var rightType in right.QueryTypes<InputObjectType>())
            {
                if (builder.TryGetType<InputObjectType>(rightType.Name, out var leftType))
                {
                    var rightTypeFields = right.GetInputFields(rightType.Name);

                    foreach (var rightTypeField in rightTypeFields)
                    {
                        builder.Connections(connect =>
                        {
                            if (connect.TryGetInputField(leftType, rightTypeField.Key, out _)) 
                                return;
                            
                            connect.IncludeInputFields(leftType, new[] {rightTypeField});
                        });
                    }
                }
            }

            foreach (var rightType in right.QueryTypes<ScalarType>())
            {
                if (!builder.TryGetType<ScalarType>(rightType.Name, out _))
                {
                    builder.Include(rightType);
                }
            }

            return builder.Build();
        }
    }
}