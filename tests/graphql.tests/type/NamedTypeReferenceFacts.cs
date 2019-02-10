using System.Threading.Tasks;
using tanka.graphql.graph;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class NamedTypeReferenceFacts
    {
        [Fact]
        public void Circular_type_reference()
        {
            /* Given */
            var person = new ObjectType(
                "Person",
                new Fields
                {
                    {"friends", new List(new NamedTypeReference("Person"))}
                });

            /* When */
            var schema = Schema.Initialize(new ObjectType("Query", new Fields
            {
                {"person", person}
            }));

            var actualPerson = schema.GetNamedType<ObjectType>(person.Name);
            var friendsField = actualPerson.GetField("friends");

            /* Then */          
            Assert.IsType<List>(friendsField.Type);
            Assert.Equal(actualPerson, ((List)friendsField.Type).WrappedType);
        }
    }
}