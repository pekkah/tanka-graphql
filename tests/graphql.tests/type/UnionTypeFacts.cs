using tanka.graphql.type;
using tanka.graphql.typeSystem;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class UnionTypeFacts
    {
        [Fact]
        public void Define_union()
        {
            /* Given */
            var builder = new SchemaBuilder();

            builder.Object("Person", out var person)
                .Field(person, "name", ScalarType.NonNullString);

            builder.Object("Photo", out var photo)
                .Field(photo, "height", ScalarType.NonNullInt)
                .Field(photo, "width", ScalarType.NonNullInt);

            /* When */
            builder.Union("SearchResult", out var searchResult,
                possibleTypes: new []{ person, photo});

            var personIsPossible = searchResult.IsPossible(person);
            var photoIsPossible = searchResult.IsPossible(photo);

            /* Then */
            Assert.True(personIsPossible);
            Assert.True(photoIsPossible);
        }
    }
}