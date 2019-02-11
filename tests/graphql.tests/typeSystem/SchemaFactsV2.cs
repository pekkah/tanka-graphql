using tanka.graphql.type;
using tanka.graphql.typeSystem;
using Xunit;

namespace tanka.graphql.tests.typeSystem
{
    public class SchemaFactsV2
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
    }
}