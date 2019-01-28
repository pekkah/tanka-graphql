using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.descriptions
{
    public class FieldFacts
    {
        [Fact]
        public void Describe()
        {
            /* Given */
            var meta = new Meta(
                "Unique ID of the object");

            /* When */
            var field = new Field(
                ScalarType.NonNullID,
                meta: meta);

            /* Then */
            Assert.Equal(field.Meta.Description, meta.Description);
        }

        [Fact]
        public void Meta_is_always_available()
        {
            /* Given */
            /* When */
            var field = new Field(
                ScalarType.NonNullID);

            /* Then */
            Assert.NotNull(field.Meta);
            Assert.Empty(field.Meta.Description);
        }
    }
}