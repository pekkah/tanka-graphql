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
            var description = "Unique ID of the object";

            /* When */
            var field = new Field(
                ScalarType.NonNullID,
                description: description);

            /* Then */
            Assert.Equal(field.Description, description);
        }

        [Fact]
        public void Meta_is_always_available()
        {
            /* Given */
            /* When */
            var field = new Field(
                ScalarType.NonNullID);

            /* Then */
            Assert.Empty(field.Description);
        }
    }
}