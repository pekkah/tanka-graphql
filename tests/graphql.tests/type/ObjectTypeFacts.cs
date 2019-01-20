using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class ObjectTypeFacts
    {
        [Fact]
        public void With_scalar_field()
        {
            /* Given */
            var person = new ObjectType(
                "Person",
                new Fields
                {
                    {"name", ScalarType.NonNullString}
                });

            /* When */
            var name = person.GetField("name");

            /* Then */
            Assert.Equal("Person", person.Name);
            Assert.NotNull(name);
            Assert.Equal(ScalarType.NonNullString, name.Type);
        }

        [Fact]
        public void With_scalar_field_with_argument()
        {
            /* Given */
            var person = new ObjectType(
                "Person",
                new Fields
                {
                    {
                        "phoneNumber", ScalarType.NonNullString, new Args
                        {
                            {"primary", ScalarType.Boolean}
                        }
                    }
                });

            /* When */
            var phoneNumber = person.GetField("phoneNumber");

            /* Then */
            Assert.NotNull(phoneNumber);
            Assert.Equal(ScalarType.NonNullString, phoneNumber.Type);
            Assert.Single(phoneNumber.Arguments,
                arg => arg.Key == "primary" && (ScalarType) arg.Value.Type == ScalarType.Boolean);
        }
    }
}