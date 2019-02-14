﻿using System;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using Xunit;
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleLiteral

namespace tanka.graphql.tests.type
{
    public class SchemaBuilderFacts
    {
        [Fact]
        public void Create_Object()
        {
            /* Given */
            var builder = new SchemaBuilder();
            const string name = "Object1";
            
            /* When */
            builder.Object(
                name: name,
                out var object1,
                description: "Description",
                interfaces: new InterfaceType[]
                {
                    /*interfaceType*/
                },
                directives: new DirectiveInstance[]
                {
                    /*directive*/
                });

            /* Then */
            Assert.Equal(name, object1.Name);
            Assert.True(builder.TryGetType<ObjectType>(object1.Name, out _));
        }

        [Fact]
        public void Create_Interface()
        {
            /* Given */
            var builder = new SchemaBuilder();
            const string name = "Interface1";
            
            /* When */
            builder.Interface(
                name: name,
                out var interface1,
                description: "Description",
                directives: new DirectiveInstance[]
                {
                    /*directive*/
                });

            /* Then */
            Assert.Equal(name, interface1.Name);
            Assert.True(builder.TryGetType<InterfaceType>(interface1.Name, out _));
        }

        [Fact]
        public void Create_Union()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Object("Object1", out var object1);
            builder.Object("Object2", out var object2);
            
            const string name = "Union1";
            
            /* When */
            builder.Union(
                name: name,
                out var union1,
                description: "Description",
                directives: new DirectiveInstance[]
                {
                    /*directive*/
                },
                possibleTypes: new []{object1, object2});

            /* Then */
            Assert.Equal(name, union1.Name);
            Assert.True(builder.TryGetType<UnionType>(union1.Name, out _));
        }

        [Fact]
        public void Create_Enum()
        {
            /* Given */
            var builder = new SchemaBuilder();
            const string name = "Enum1";
            
            /* When */
            builder.Enum(
                name: name,
                enumType: out var enum1,
                description: "Description",
                directives: new DirectiveInstance[]
                {
                    /*directive*/
                },
                (value: "VALUE1", 
                 description: "Description",
                 directives: new DirectiveInstance[] { /*directive*/},
                 deprecationReason: null)
                );

            /* Then */
            Assert.Equal(name, enum1.Name);
            Assert.True(builder.TryGetType<EnumType>(enum1.Name, out _));
        }

        [Fact]
        public void Create_Scalar()
        {
            /* Given */
            var builder = new SchemaBuilder();
            const string name = "Url";
            
            /* When */
            builder.Scalar(
                name: name,
                out var url,
                converter: new StringConverter(),
                description: "Description",
                directives: new DirectiveInstance[]
                {
                    /*directive*/
                });

            /* Then */
            Assert.Equal(name, url.Name);
            Assert.True(builder.TryGetType<ScalarType>(url.Name, out _));
        }

        [Fact]
        public void Create_InputObject()
        {
            /* Given */
            var builder = new SchemaBuilder();
            const string name = "InputObject1";
            
            /* When */
            builder.InputObject(
                name: name,
                out var object1,
                description: "Description",
                directives: new DirectiveInstance[]
                {
                    /*directive*/
                });

            /* Then */
            Assert.Equal(name, object1.Name);
            Assert.True(builder.TryGetType<InputObjectType>(object1.Name, out _));
        }

        [Fact]
        public void Create_DirectiveType()
        {
            /* Given */
            var builder = new SchemaBuilder();
            const string name = "Deprecated";
            
            /* When */
            builder.DirectiveType(
                name: name,
                out var object1,
                locations: new []
                {
                    DirectiveLocation.FIELD
                },
                description: "Description",
                (Name: "Reason",
                 Type: ScalarType.String,
                 DefaultValue: "Deprecated",
                 Description: "Description")
                );

            /* Then */
            Assert.Equal(name, object1.Name);
        }

        [Fact]
        public void Build_and_validate_schema()
        {
            /* Given */
            var builder = new SchemaBuilder()
                // query is required to build schema
                .Query(out _);

            /* When */
            var (schema, validationResult) = builder.BuildAndValidate();

            /* Then */
            Assert.IsAssignableFrom<ISchema>(schema);
            Assert.IsType<SchemaGraph>(schema);
            Assert.NotNull(schema.Query);
            Assert.IsType<NotImplementedException>(validationResult);
        }

        [Fact]
        public void Create_Object_field()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Object(
                name: "Object1",
                out var object1);
            
            /* When */
            builder.Connections(connect =>
            {
                connect.Field(
                    owner: object1,
                    fieldName: "field1",
                    to: ScalarType.Int,
                    description: "Description",
                    directives: new DirectiveInstance[]
                    {
                        /* directive */
                    },
                    (Name: "arg1",
                     Type: ScalarType.Boolean,
                     DefaultValue: true,
                     Description: "Description")
                    );
            });

            /* Then */
            var isDefined = false;
            builder.Connections(connect => isDefined = connect.TryGetField(object1, "field1", out _));
            Assert.True(isDefined);
        }

        [Fact]
        public void Create_Interface_field()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Interface(
                name: "Interface1",
                out var interface1);

            /* When */
            builder.Connections(connect =>
            {
                connect.Field(
                    owner: interface1,
                    fieldName: "field1",
                    to: ScalarType.Int,
                    description: "Description",
                    directives: new DirectiveInstance[]
                    {
                        /* directive */
                    },
                    (Name: "arg1",
                     Type: ScalarType.Boolean,
                     DefaultValue: true,
                     Description: "Description")
                );
            });

            /* Then */
            var isDefined = false;
            builder.Connections(connect => isDefined = connect.TryGetField(interface1, "field1", out _));
            Assert.True(isDefined);
        }

        [Fact]
        public void Create_InputObject_field()
        {
            /* Given */
            var builder = new SchemaBuilder();        
            builder.InputObject(
                name: "Input1",
                out var input1);

            /* When */
            builder.Connections(connect => connect
                .InputField(
                    owner: input1,
                    fieldName: "field1",
                    to: ScalarType.NonNullBoolean,
                    defaultValue: true,
                    description: "Descriptipn",
                    directives: new DirectiveInstance[]
                    {
                        /* directive */
                    })
            );


            /* Then */
            var isDefined = false;
            builder.Connections(connect => isDefined = connect.TryGetInputField(input1, "field1", out _));
            Assert.True(isDefined);
        }

        [Fact]
        public void Use_existing_schema()
        {
            /* Given */
            var existingSchema = new SchemaBuilder()
                .Query(out var query)
                .Connections(connect =>
                    connect.Field(query, "field1", ScalarType.String)
                    )
                .Build();

            /* When */
            var schema = new SchemaBuilder(existingSchema)
                /* Or: .Import(existingSchema) */
                .Connections(connect => connect
                    .Field(query, "field2", ScalarType.Int)
                )
                .Build();

            /* Then */
            var queryFields = schema.GetFields(query.Name).ToList();
            Assert.Single(queryFields, f => f.Key == "field1");
            Assert.Single(queryFields, f => f.Key == "field2");
        }

        [Fact]
        public void Merge_schemas()
        {
            /* Given */
            var schema1 = new SchemaBuilder()
                .Query(out var query1)
                .Connections(connect =>
                    connect.Field(query1, "field1", ScalarType.String)
                )
                .Build();

            var schema2 = new SchemaBuilder()
                .Query(out var query2)
                .Connections(connect =>
                    connect.Field(query2, "field2", ScalarType.Int)
                )
                .Build();

            /* When */
            var schema = MergeTool.MergeSchemas(schema1, schema2);

            /* Then */
            var queryFields = schema.GetFields(query1.Name).ToList();
            Assert.Single(queryFields, f => f.Key == "field1");
            Assert.Single(queryFields, f => f.Key == "field2");
        }

        [Fact]
        public async Task Make_executable_schema()
        {
            /* Given */
            var schema1 = new SchemaBuilder()
                .Query(out var query1)
                .Connections(connect =>
                    connect.Field(query1, "field1", ScalarType.Int)
                )
                .Build();

            var resolvers = new ResolverMap()
            {
                {
                    query1.Name, new FieldResolverMap()
                    {
                        { "field1", async context =>
                            {
                                await Task.Delay(1);
                                return Resolve.As(1);
                            }
                        }
                    }

                }
            };

            /* When */
            var executableSchema = await SchemaTools.MakeExecutableSchemaAsync(
                schema: schema1,
                resolvers: resolvers,
                subscribers: null,
                visitors: new SchemaVisitorFactory[]
                {
                    /* schema visitor factory */
                });

            /* Then */
            var result = await Executor.ExecuteAsync(
                new ExecutionOptions()
                {
                    Document = Parser.ParseDocument(@"{ field1 }"),
                    Schema = executableSchema
                });

            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""field1"": 1
                  }
                }");
        }

        [Fact]
        public void Build_with_circular_reference_between_two_objects()
        {
            /* Given */
            var builder = new SchemaBuilder();

            /* When */
            var schema = builder
                .Object("Object1", out var obj1)
                .Object("Object2", out var obj2)
                .Connections(connect => connect
                    .Field(obj1, "obj1-obj2", obj2)
                    .Field(obj2, "obj2-obj1", obj1)
                    .Field(obj1, "scalar", ScalarType.Int))
                .Query(out var query)
                .Connections(connect => connect
                    .Field(query, "query-obj1", obj1))
                .Build();

            /* Then */
            var object1 = schema.GetNamedType<ObjectType>(obj1.Name);
            var object1ToObject2 = schema.GetField(object1.Name, "obj1-obj2");

            var object2 = schema.GetNamedType<ObjectType>(obj2.Name);
            var object2ToObject1 = schema.GetField(object2.Name, "obj2-obj1");

            Assert.Equal(object1, object2ToObject1.Type);
            Assert.Equal(object2, object1ToObject2.Type);
        }

        [Fact]
        public void Build_types()
        {
            /* Given */
            var builder = new SchemaBuilder();

            builder.Object("Object1", out var object1);
            builder.Query(out var query);

            builder.Connections(connect => connect
                .Field(object1, "field1", ScalarType.Float)
                .Field(query, "field1", object1));

            /* When */
            var sut = builder.Build();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            Assert.Contains(types, t => t.Name == "Query");
            Assert.Contains(types, t => t.Name == "Object1");
        }

        [Fact]
        public void Requires_Query()
        {
            /* Given */
            var builder = new SchemaBuilder();

            /* When */
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Build());

            /* Then */
            Assert.Equal("types", exception.ParamName);
        }

        [Fact]
        public void Create_Mutation()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Query(out _);
            builder.Mutation(out var mutation);

            /* When */
            var sut = builder.Build();

            /* Then */
            Assert.Equal(mutation, sut.Mutation);
        }

        [Fact]
        public void Create_Query()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Query(out var query);

            /* When */
            var sut = builder.Build();

            /* Then */
            Assert.Equal(query, sut.Query);
        }

        [Fact]
        public void Create_Subscription()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Query(out _);
            builder.Mutation(out _);
            builder.Subscription(out var subscription);

            /* When */
            var sut = builder.Build();

            /* Then */
            Assert.Equal(subscription, sut.Subscription);
        }
    }
}