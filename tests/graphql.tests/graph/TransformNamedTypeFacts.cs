using System.Threading.Tasks;
using tanka.graphql.graph;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.graph
{
    public class TransformNamedTypeFacts
    {
        [Fact]
        public async Task Rename_ObjectType_contained_by_nested_object_type_query()
        {
            /* Given */
            const string originalName = "OriginalName";
            const string newName = "NewName";

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

            var schema = await new Schema(query).InitializeAsync();

            /* When */
            var transform = new TransformNamedType(namedType =>
            {
                if (namedType.Name == originalName) 
                    return namedType.WithName(newName);

                return namedType;
            });

            var newSchema = transform.Transform(schema);
            await newSchema.InitializeAsync();

            /* Then */
            Assert.Null(newSchema.GetNamedType(originalName));
            Assert.NotNull(newSchema.GetNamedType(newName));
            var containerField = newSchema.Query.GetFieldWithKey("container");
            var actualContainer = (ObjectType) containerField.Value.Type;
            var objectField = actualContainer.GetFieldWithKey("object");
            Assert.Equal(newName, ((ObjectType) objectField.Value.Type).Name);
        }

        [Fact]
        public async Task Delete_ObjectType_contained_by_nested_object_type_query()
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

            var schema = await new Schema(query).InitializeAsync();

            /* When */
            var transform = new TransformNamedType(namedType =>
            {
                if (namedType.Name == originalName)
                    return null;

                return namedType;
            });

            var newSchema = transform.Transform(schema);
            await newSchema.InitializeAsync();

            /* Then */
            Assert.Null(newSchema.GetNamedType(originalName));

            var containerField = newSchema.Query.GetFieldWithKey("container");
            var actualContainer = (ObjectType) containerField.Value.Type;
            var objectField = actualContainer.GetFieldWithKey("object");
            Assert.Null(objectField.Value.Type);
        }

        [Fact]
        public async Task Rename_ObjectType_contained_by_query()
        {
            /* Given */
            const string originalName = "OriginalName";
            const string newName = "NewName";

            var object1 = new ObjectType(
                originalName,
                new Fields());

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"object", object1}
                });

            var schema = await new Schema(query).InitializeAsync();

            /* When */
            var transform = new TransformNamedType(namedType =>
            {
                if (namedType.Name == originalName) return namedType.WithName(newName);

                return namedType;
            });

            var newSchema = transform.Transform(schema);
            await newSchema.InitializeAsync();

            /* Then */
            Assert.Null(newSchema.GetNamedType(originalName));
            Assert.NotNull(newSchema.GetNamedType(newName));

            var objectField = newSchema.Query.GetFieldWithKey("object");
            Assert.Equal(newName, ((ObjectType) objectField.Value.Type).Name);
        }

        [Fact]
        public async Task Rename_ObjectType_in_a_list()
        {
            /* Given */
            const string originalName = "OriginalName";
            const string newName = "NewName";

            var object1 = new ObjectType(
                originalName,
                new Fields());

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"object", new List(object1)}
                });

            var schema = await new Schema(query).InitializeAsync();

            /* When */
            var transform = new TransformNamedType(namedType =>
            {
                if (namedType.Name == originalName) 
                    return namedType.WithName(newName);

                return namedType;
            });

            var newSchema = transform.Transform(schema);
            await newSchema.InitializeAsync();

            /* Then */
            Assert.Null(newSchema.GetNamedType(originalName));
            Assert.NotNull(newSchema.GetNamedType(newName));

            var objectField = newSchema.Query.GetFieldWithKey("object");
            Assert.IsType<List>(objectField.Value.Type);
            Assert.Equal(newName, ((ObjectType) objectField.Value.Type.Unwrap()).Name);
        }
    }
}