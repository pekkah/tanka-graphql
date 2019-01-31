using System.Threading.Tasks;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class NamedTypeReferenceFacts
    {
        [Fact]
        public async Task Circular_type_reference()
        {
            /* Given */
            var person = new ObjectType(
                "Person",
                new Fields
                {
                    {"friends", new List(new NamedTypeReference("Person"))}
                });

            /* When */
            var schema = new Schema(new ObjectType("Query", new Fields
            {
                {"person", person}
            }));

            await schema.InitializeAsync();
            var friendsField = schema.GetNamedType<ObjectType>(person.Name)
                .GetField("friends");

            /* Then */          
            Assert.IsType<List>(friendsField.Type);
            Assert.Equal(person, ((List)friendsField.Type).WrappedType);
        }
    }
}