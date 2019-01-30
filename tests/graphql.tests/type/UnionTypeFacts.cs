using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class UnionTypeFacts
    {
        [Fact]
        public void Define_union()
        {
            /* Given */
            var person = new ObjectType(
                "Person",
                new Fields
                {
                    {"name", ScalarType.NonNullString}
                });

            var photo = new ObjectType(
                "Photo",
                new Fields
                {
                    {"height", ScalarType.NonNullInt},
                    {"width", ScalarType.NonNullInt}
                });

            /* When */
            var searchResult = new UnionType(
                "SearchResult",
                new[] {person, photo});

            var personIsPossible = searchResult.IsPossible(person);
            var photoIsPossible = searchResult.IsPossible(photo);

            /* Then */
            Assert.True(personIsPossible);
            Assert.True(photoIsPossible);
        }
    }
}