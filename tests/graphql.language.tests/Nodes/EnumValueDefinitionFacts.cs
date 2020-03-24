using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class EnumValueDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            EnumValueDefinition original = "V1";

            /* Then */
            Assert.Equal("V1", original.Value.Value);
        }
    }
}