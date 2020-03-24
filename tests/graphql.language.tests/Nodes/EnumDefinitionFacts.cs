using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class EnumDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            EnumDefinition original = "enum ENUM { V1, V2 }";

            /* Then */
            Assert.Equal("ENUM", original.Name);
        }
    }
}