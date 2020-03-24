using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class InputValueDefinitionFacts
    {
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            InputValueDefinition original = "field: ENUM";

            /* Then */
            Assert.Equal("field", original.Name);
            Assert.IsType<NamedType>(original.Type);
        }

        [Fact]
        public void WithDescription()
        {
            /* Given */
            InputValueDefinition original = "field: ENUM!";

            /* When */
            var modified = original
                .WithDescription("Description");

            /* Then */
            Assert.Equal("Description", modified.Description);
        }
    }
}