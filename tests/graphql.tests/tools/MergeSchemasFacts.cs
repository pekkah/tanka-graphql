using System.Threading.Tasks;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.tools
{
    public class MergeSchemasFacts
    {
        [Fact]
        public async Task should_include_fields_if_no_conflict()
        {
            /* Given */
            var left = Schema.Initialize(new ObjectType("Q", new Fields
            {
                ["left"] = new Field(ScalarType.Int)
            }));

            var right = Schema.Initialize(new ObjectType("Q", new Fields
            {
                ["right"] = new Field(ScalarType.String)
            }));


            /* When */
            var mergedSchema = MergeTool.MergeSchemas(left, right, (l, r) => r.Field);

            /* Then */
            Assert.Single(mergedSchema.Query.Fields, pair => pair.Key == "left");
            Assert.Single(mergedSchema.Query.Fields, pair => pair.Key == "right");
        }

        [Fact]
        public async Task should_resolve_conflict_with_resolver()
        {
            /* Given */
            var left = Schema.Initialize(new ObjectType("Q", new Fields
            {
                ["left"] = new Field(ScalarType.Int)
            }));

            var right = Schema.Initialize(new ObjectType("Q", new Fields
            {
                ["left"] = new Field(ScalarType.String)
            }));
            
            /* When */
            var mergedSchema = MergeTool.MergeSchemas(left, right, (l, r) => r.Field);

            /* Then */
            Assert.Single(mergedSchema.Query.Fields, pair => pair.Key == "left" && (ScalarType) pair.Value.Type == ScalarType.String);
        }
    }
}