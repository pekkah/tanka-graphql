using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class DirectiveFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            Directive original = "@a(x: 100, y: 100)";

            /* Then */
            Assert.Equal("a", original.Name);
            Assert.NotNull(original.Arguments);
            Assert.Equal(2, original.Arguments?.Count);
        }
    }
}