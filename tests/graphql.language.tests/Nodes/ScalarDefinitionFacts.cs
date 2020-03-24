using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class ScalarDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            ScalarDefinition original = "scalar Name";

            /* Then */
            Assert.Equal("Name", original.Name);
        }
    }
}