using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class TypeDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            TypeDefinition original = "enum Enum";

            /* Then */
            Assert.IsType<EnumDefinition>(original);
        }
    }
}