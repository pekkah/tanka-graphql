using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class NonNullFacts
    {
        [Fact]
        public void Define_NonNull()
        {
            /* Given */
            var itemType = ScalarType.Int;

            /* When */
            var nonNull = new NonNull(itemType);

            /* Then */
            Assert.Equal(itemType, nonNull.OfType);
        }
    }
}