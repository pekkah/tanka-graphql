using System.Threading.Tasks;
using fugu.graphql.tools;
using fugu.graphql.type;
using Xunit;

namespace fugu.graphql.tests.tools
{
    public class MergeSchemasFacts
    {
        [Fact]
        public async Task should_include_fields_if_no_conflict()
        {
            /* Given */
            var left = new Schema(new ObjectType("Q", new Fields
            {
                ["left"] = new Field(ScalarType.Int)
            }));
            await left.InitializeAsync();

            var right = new Schema(new ObjectType("Q", new Fields
            {
                ["right"] = new Field(ScalarType.String)
            }));
            await right.InitializeAsync();


            /* When */
            var mergedSchema = MergeTool.MergeSchemas(left, right, (l, r) => r.Field);
            await mergedSchema.InitializeAsync();

            /* Then */
            Assert.Single(mergedSchema.Query.Fields, pair => pair.Key == "left");
            Assert.Single(mergedSchema.Query.Fields, pair => pair.Key == "right");
        }

        [Fact]
        public async Task should_resolve_conflict_with_resolver()
        {
            /* Given */
            var left = new Schema(new ObjectType("Q", new Fields
            {
                ["left"] = new Field(ScalarType.Int)
            }));
            await left.InitializeAsync();

            var right = new Schema(new ObjectType("Q", new Fields
            {
                ["left"] = new Field(ScalarType.String)
            }));
            await right.InitializeAsync();


            /* When */
            var mergedSchema = MergeTool.MergeSchemas(left, right, (l, r) => r.Field);
            await mergedSchema.InitializeAsync();

            /* Then */
            Assert.Single(mergedSchema.Query.Fields, pair => pair.Key == "left" && (ScalarType) pair.Value.Type == ScalarType.String);
        }
    }
}