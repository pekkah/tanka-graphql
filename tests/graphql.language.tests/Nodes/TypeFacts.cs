using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class TypeFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            Type original = "String";

            /* Then */
            Assert.IsType<NamedType>(original);
        }
    }
}