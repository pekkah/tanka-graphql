using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Introspection
{
    public class IntrospectionSchemaReaderFacts
    {
        [Fact]
        public void Read_Enum_with_values()
        {
            /* Given */
            var introspectedType = new __Type
            {
                Kind = __TypeKind.ENUM,
                Name = "T",
                EnumValues = new List<__EnumValue>
                {
                    new __EnumValue
                    {
                        Name = "VALUE1",
                        Description = "description"
                    }
                }
            };

            var schema = new __Schema
            {
                Types = new List<__Type>
                {
                    introspectedType
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            Assert.True(builder.TryGetType<EnumType>(introspectedType.Name, out var type));
            Assert.Single(type.Values, value => value.Key == "VALUE1");
        }

        [Fact]
        public void Read_InputObjectType_with_field()
        {
            /* Given */
            var type = new __Type
            {
                Kind = __TypeKind.INPUT_OBJECT,
                Name = "object",
                InputFields = new List<__InputValue>
                {
                    new __InputValue
                    {
                        Name = "field1",
                        Type = new __Type
                        {
                            Kind = __TypeKind.SCALAR,
                            Name = "Int"
                        }
                    }
                }
            };

            var schema = new __Schema
            {
                Types = new List<__Type>
                {
                    type
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            builder.TryGetType<InputObjectType>(type.Name, out var inputObjectType);
            Assert.NotNull(inputObjectType);
            builder.Connections(connections =>
            {
                Assert.True(connections.TryGetInputField(inputObjectType, "field1", out var field1));
                Assert.Equal(ScalarType.Int, field1.Type);
            });
        }

        [Fact]
        public void Read_InterfaceType_with_field()
        {
            /* Given */
            var type = new __Type
            {
                Kind = __TypeKind.INTERFACE,
                Name = "object",
                Fields = new List<__Field>
                {
                    new __Field
                    {
                        Name = "field1",
                        Type = new __Type
                        {
                            Kind = __TypeKind.SCALAR,
                            Name = "Int"
                        },
                        Args = new List<__InputValue>
                        {
                            new __InputValue
                            {
                                Name = "arg1",
                                Type = new __Type
                                {
                                    Kind = __TypeKind.SCALAR,
                                    Name = "String"
                                }
                            }
                        }
                    }
                }
            };

            var schema = new __Schema
            {
                Types = new List<__Type>
                {
                    type
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            Assert.True(builder.TryGetType<InterfaceType>(type.Name, out var interfaceType));

            builder.Connections(connections =>
            {
                Assert.True(connections.TryGetField(interfaceType, "field1", out var field1));
                Assert.Equal(ScalarType.Int, field1.Type);
                Assert.Single(field1.Arguments, arg => arg.Key == "arg1"
                                                       && ScalarType.String.Equals(arg.Value.Type));
            });
        }

        [Fact]
        public void Read_MutationType()
        {
            /* Given */
            var mutation = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = "M"
            };

            var schema = new __Schema
            {
                MutationType = mutation,
                Types = new List<__Type>
                {
                    mutation
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            builder.TryGetType<ObjectType>("Mutation", out var mutationType);
            Assert.NotNull(mutationType);
        }

        [Fact]
        public void Read_ObjectType_with_field()
        {
            /* Given */
            var type = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = "object",
                Fields = new List<__Field>
                {
                    new __Field
                    {
                        Name = "field1",
                        Type = new __Type
                        {
                            Kind = __TypeKind.SCALAR,
                            Name = "Int"
                        },
                        Args = new List<__InputValue>
                        {
                            new __InputValue
                            {
                                Name = "arg1",
                                Type = new __Type
                                {
                                    Kind = __TypeKind.SCALAR,
                                    Name = "String"
                                }
                            }
                        }
                    }
                }
            };

            var schema = new __Schema
            {
                Types = new List<__Type>
                {
                    type
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            builder.TryGetType<ObjectType>(type.Name, out var objectType);
            Assert.NotNull(objectType);
            builder.Connections(connections =>
            {
                Assert.True(connections.TryGetField(objectType, "field1", out var field1));
                Assert.Equal(ScalarType.Int, field1.Type);
                Assert.Single(field1.Arguments, arg => arg.Key == "arg1"
                                                       && ScalarType.String.Equals(arg.Value.Type));
            });
        }

        [Fact]
        public void Read_QueryType()
        {
            /* Given */
            var query = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = "Q"
            };

            var schema = new __Schema
            {
                QueryType = query,
                Types = new List<__Type>
                {
                    query
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            builder.TryGetType<ObjectType>("Query", out var queryType);
            Assert.NotNull(queryType);
        }

        [Fact]
        public void Read_Scalars()
        {
            /* Given */
            var types = ScalarType.Standard
                .Select(scalar => new __Type
                {
                    Kind = __TypeKind.SCALAR,
                    Name = scalar.Name
                }).ToList();

            var schema = new __Schema
            {
                Types = types
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            foreach (var scalarType in ScalarType.Standard)
                Assert.True(builder.TryGetType<ScalarType>(scalarType.Name, out _));
        }

        [Fact]
        public void Read_SubscriptionType()
        {
            /* Given */
            var subscription = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = "S"
            };

            var schema = new __Schema
            {
                SubscriptionType = subscription,
                Types = new List<__Type>
                {
                    subscription
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            builder.TryGetType<ObjectType>("Subscription", out var subscriptionType);
            Assert.NotNull(subscriptionType);
        }

        [Fact]
        public void Read_UnionType()
        {
            /* Given */
            var object1 = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = "object1"
            };
            var object2 = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = "object2"
            };
            var introspectedType = new __Type
            {
                Kind = __TypeKind.UNION,
                Name = "U",
                PossibleTypes = new List<__Type>
                {
                    object1,
                    object2
                }
            };

            var schema = new __Schema
            {
                Types = new List<__Type>
                {
                    object1,
                    introspectedType,
                    object2
                }
            };

            var builder = new SchemaBuilder();
            var reader = new IntrospectionSchemaReader(
                builder,
                new IntrospectionResult
                {
                    Schema = schema
                });

            /* When */
            reader.Read();

            /* Then */
            Assert.True(builder.TryGetType<UnionType>(introspectedType.Name, out var type));
            Assert.Single(type.PossibleTypes, possibleType => possibleType.Key == "object1");
            Assert.Single(type.PossibleTypes, possibleType => possibleType.Key == "object2");
        }
    }
}