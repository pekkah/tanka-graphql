using tanka.graphql.graph;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.graph
{
    public class DeleteTransformFacts
    {
        [Fact]
        public void Delete_ObjectType_contained_by_nested_object_type_query()
        {
            /* Given */
            const string originalName = "OriginalName";

            var object1 = new ObjectType(
                originalName,
                new Fields());

            var container = new ObjectType(
                "Container",
                new Fields
                {
                    {"object", object1}
                });

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"container", container}
                });

            var schema = Schema.Initialize(query);

            /* When */
            var newSchema = Transforms.Apply(schema, Transforms.Delete(originalName));

            /* Then */
            Assert.Null(newSchema.GetNamedType(originalName));

            var containerField = newSchema.Query.GetFieldWithKey("container");
            var actualContainer = (ObjectType) containerField.Value.Type;

            Assert.False(actualContainer.HasField(originalName));
            Assert.Null(newSchema.GetNamedType(originalName));
        }
    }
}