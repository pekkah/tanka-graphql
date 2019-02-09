using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.graph;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.graph
{
    public class TransformNamedType_HealTypeReference
    {
        [Fact]
        public void Heal_Interface_field_with_self_reference()
        {
            /* Given */
            const string originalName = "OriginalName";
            var interface1 = new InterfaceType(
                originalName,
                new Fields
                {
                    {"object", new NamedTypeReference(originalName)}
                });

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"object", interface1}
                });

            /* When */
            var newSchema = Schema.Initialize(query);

            /* Then */
            var queryObjectField = newSchema.Query.GetField("object");
            var objectObjectField = ((InterfaceType)queryObjectField.Type)
                .GetField("object");

            Assert.IsType<InterfaceType>(objectObjectField.Type);
            Assert.Same(newSchema.GetNamedType(originalName), objectObjectField.Type);
        }

        [Fact]
        public async Task Heal_Object_field_type()
        {
            /* Given */
            const string originalName = "OriginalName";

            var object2 = new ObjectType(
                "object2",
                new Fields());

            var object1 = new ObjectType(
                originalName,
                new Fields
                {
                    {"object", new NamedTypeReference(object2.Name)}
                });

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"object", object1}
                });

            /* When */
            var newSchema = Schema.Initialize(query, byNameOnly: new [] { object2});

            /* Then */
            var queryObjectField = newSchema.Query.GetFieldWithKey("object");
            var actualObject1 = (ObjectType)queryObjectField.Value.Type;
            var objectObjectField = actualObject1
                .GetField("object");

            Assert.IsType<ObjectType>(objectObjectField.Type);
            Assert.Same(object2, objectObjectField.Type);
        }

        [Fact]
        public void Heal_Object_field_with_self_references()
        {
            /* Given */
            const string originalName = "OriginalName";
            var object1 = new ObjectType(
                originalName,
                new Fields
                {
                    {"field1", new NamedTypeReference(originalName)}
                });

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"field1", object1},
                    {"field2", object1}
                });

            /* When */
            var newSchema = Schema.Initialize(query);

            /* Then */
            var queryObjectField1 = newSchema.Query.GetField("field1");
            var queryObjectField2 = newSchema.Query.GetField("field2");

            Assert.IsType<ObjectType>(queryObjectField1.Type);
            Assert.Same(queryObjectField1.Type, queryObjectField2.Type);
            Assert.Equal(2, newSchema.QueryTypes<INamedType>().Count());
        }
    }
}