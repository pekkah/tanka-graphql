using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class InterfaceTypeFacts
    {
        [Fact]
        public void Define_interface()
        {
            /* Given */
            /* When */
            var namedEntity = new InterfaceType(
                "NamedEntity",
                new Fields
                {
                    {"name", ScalarType.NonNullString}
                });


            /* Then */
            Assert.Equal("NamedEntity", namedEntity.Name);
            Assert.Single(namedEntity.Fields, fk => fk.Key == "name"
                                                    && (NonNull) fk.Value.Type == ScalarType.NonNullString);
        }

        [Fact]
        public void Implement_interface()
        {
            /* Given */
            var namedEntity = new InterfaceType(
                "NamedEntity",
                new Fields
                {
                    {"name", ScalarType.NonNullString}
                });

            var person = new ObjectType(
                "Person",
                new Fields
                {
                    {"name", ScalarType.NonNullString}
                },
                implements: new[] {namedEntity});

            /* When */
            var implements = person.Implements(namedEntity);

            /* Then */
            Assert.True(implements);
        }
    }
}