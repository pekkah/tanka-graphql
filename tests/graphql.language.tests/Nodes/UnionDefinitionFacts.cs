using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class UnionDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            UnionDefinition original = "union Name = MemberA | MemberB";

            /* Then */
            Assert.Equal("Name", original.Name);
            Assert.Equal(2, original.Members?.Count);
        }
    }
}