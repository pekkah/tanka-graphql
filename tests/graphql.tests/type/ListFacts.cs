using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class ListFacts
    {
        [Fact]
        public void Define_list()
        {
            /* Given */
            var itemType = ScalarType.Int;

            /* When */
            var list = new List(itemType);

            /* Then */
            Assert.Equal(itemType, list.OfType);
        }
    }
}