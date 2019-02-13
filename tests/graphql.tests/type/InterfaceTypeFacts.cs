using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class InterfaceTypeFacts
    {
        private SchemaBuilder _builder;

        public InterfaceTypeFacts()
        {
            _builder = new SchemaBuilder();
            _builder.Query(out _);
        }

        [Fact]
        public void Define_interface()
        {
            /* Given */
            /* When */
            _builder.Interface("NamedEntity", out var namedEntity)
                .Connections(connect => connect
                .Field(namedEntity, "name", ScalarType.NonNullString));

            var schema = _builder.Build();

            /* Then */
            var namedEntityFields = schema.GetFields(namedEntity.Name);
            Assert.Equal("NamedEntity", namedEntity.Name);
            Assert.Single(namedEntityFields, fk => fk.Key == "name"
                                                    && (NonNull) fk.Value.Type == ScalarType.NonNullString);
        }

        [Fact]
        public void Implement_interface()
        {
            /* Given */
            _builder.Interface("NamedEntity", out var namedEntity)
                .Connections(connect => connect
                .Field(namedEntity, "name", ScalarType.NonNullString));

            _builder.Object("Person", out var person, interfaces: new []{namedEntity})
                .Connections(connect => connect
                .Field(person, "name", ScalarType.NonNullString));

            /* When */
            //todo: interfaces should be behind a connection
            var implements = person.Implements(namedEntity);

            /* Then */
            Assert.True(implements);
        }
    }
}