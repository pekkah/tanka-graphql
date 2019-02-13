using System;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class SchemaBuilderFacts
    {
        [Fact]
        public void Build_with_circular_reference_between_two_objects()
        {
            /* Given */
            var builder = new SchemaBuilder();

            /* When */
            var schema = builder
                .Object("Object1", out var obj1)
                .Object("Object2", out var obj2)
                .Field(obj1, "obj1-obj2", obj2)
                .Field(obj2, "obj2-obj1", obj1)
                .Field(obj1, "scalar", ScalarType.Int)
                .Query(out var query)
                .Field(query, "query-obj1", obj1)
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

            builder.Object("Object1", out var object1)
                .Field(object1, "field1", ScalarType.Float);

            builder.Query(out var query)
                .Field(query, "field1", object1);

            /* When */
            var sut = builder.Build();

            /* Then */
            var types = sut.QueryTypes<INamedType>();

            Assert.Contains(types, t => t.Name == "Query");
            Assert.Contains(types, t => t.Name == "Object1");
        }

        [Fact]
        public void Require_Query()
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
        public void Set_Mutation()
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
        public void Set_Query()
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
        public void Set_Subscription()
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