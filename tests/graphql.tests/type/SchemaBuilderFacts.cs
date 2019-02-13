using System;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using Xunit;
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable ArgumentsStyleNamedExpression

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
            Assert.True(builder.IsPredefinedType<ObjectType>(object1.Name, out _));
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
            Assert.True(builder.IsPredefinedType<InterfaceType>(interface1.Name, out _));
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
            Assert.True(builder.IsPredefinedType<UnionType>(union1.Name, out _));
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
            Assert.True(builder.IsPredefinedType<EnumType>(enum1.Name, out _));
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
            Assert.True(builder.IsPredefinedType<ScalarType>(url.Name, out _));
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
            Assert.True(builder.IsPredefinedType<InputObjectType>(object1.Name, out _));
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