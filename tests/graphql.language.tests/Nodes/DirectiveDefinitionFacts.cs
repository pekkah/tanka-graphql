using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class DirectiveDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            DirectiveDefinition original = "directive @a(x: Int, y: Float) on FIELD";

            /* Then */
            Assert.Equal("a", original.Name);
        }
    }
}