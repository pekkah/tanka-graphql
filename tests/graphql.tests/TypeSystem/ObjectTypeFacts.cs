using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class ObjectTypeFacts
    {
        public ObjectTypeFacts()
        {
            _builder = new SchemaBuilder();
            _builder.Query(out _);
        }

        private readonly SchemaBuilder _builder;

        [Fact]
        public void With_scalar_field()
        {
            /* Given */
            _builder.Object("Person", out var person)
                .Connections(connect => connect
                    .Field(person, "name", ScalarType.NonNullString));

            var schema = _builder.Build();

            /* When */
            var name = schema.GetField(person.Name, "name");

            /* Then */
            Assert.Equal("Person", person.Name);
            Assert.NotNull(name);
            Assert.Equal(ScalarType.NonNullString, name.Type);
        }

        [Fact]
        public void With_scalar_field_with_argument()
        {
            /* Given */
            _builder.Object("Person", out var person)
                .Connections(connect => connect
                    .Field(person, "phoneNumber", ScalarType.NonNullString,
                        args: args => args.Arg(
                            "primary", 
                            ScalarType.Boolean, 
                            default, 
                            default
                            )
                        )
                );

            var schema = _builder.Build();

            /* When */
            var phoneNumber = schema.GetField(person.Name, "phoneNumber");

            /* Then */
            Assert.NotNull(phoneNumber);
            Assert.Equal(ScalarType.NonNullString, phoneNumber.Type);
            Assert.Single(phoneNumber.Arguments,
                arg => arg.Key == "primary" && (ScalarType) arg.Value.Type == ScalarType.Boolean);
        }
    }
}