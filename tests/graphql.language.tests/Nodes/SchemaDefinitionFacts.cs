using System.Linq;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class SchemaDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            SchemaDefinition original = "schema { query: Query }";

            /* Then */
            Assert.Equal("Query", original.Operations.Single().NamedType.Name);
        }
    }
}