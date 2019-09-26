using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
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