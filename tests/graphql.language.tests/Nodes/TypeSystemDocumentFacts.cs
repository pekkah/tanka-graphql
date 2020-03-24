using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class TypeSystemDocumentFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            TypeSystemDocument original = "scalar Scalar extend scalar Scalar @a";

            /* Then */
            Assert.NotNull(original.TypeDefinitions);
            Assert.Single(original.TypeDefinitions);

            Assert.NotNull(original.TypeExtensions);
            Assert.Single(original.TypeExtensions);
        }
    }
}