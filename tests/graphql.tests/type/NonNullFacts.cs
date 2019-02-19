using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
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
            Assert.Equal(itemType, nonNull.WrappedType);
        }
    }
}