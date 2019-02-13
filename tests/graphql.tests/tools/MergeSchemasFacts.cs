using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.tools
{
    public class MergeSchemasFacts
    {
        [Fact]
        public void should_include_fields_if_no_conflict()
        {
            /* Given */
            var left = new SchemaBuilder()
                .Query(out var leftQuery)
                .Field(leftQuery, "left", ScalarType.Int)
                .Build();

            var right = new SchemaBuilder()
                .Query(out var rightQuery)
                .Field(rightQuery, "right", ScalarType.String)
                .Build();


            /* When */
            var mergedSchema = MergeTool.MergeSchemas(left, right);
            var queryFields = mergedSchema.GetFields(mergedSchema.Query.Name)
                .ToList();

            /* Then */
            Assert.Single(queryFields, pair => pair.Key == "left");
            Assert.Single(queryFields, pair => pair.Key == "right");
        }
    }
}