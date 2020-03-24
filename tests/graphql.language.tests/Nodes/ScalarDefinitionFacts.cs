using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
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

        [Fact]
        public void WithDescription()
        {
            /* Given */
            ScalarDefinition original = @"scalar Name";

            /* When */
            var modified = original
                .WithDescription("Description");

            /* Then */
            Assert.Null(original.Description);
            Assert.Equal("Description", modified.Description);
        }

        [Fact]
        public void WithName()
        {
            /* Given */
            ScalarDefinition original = @"scalar Name";

            /* When */
            var modified = original
                .WithName("Renamed");

            /* Then */
            Assert.Equal("Name", original.Name);
            Assert.Equal("Renamed", modified.Name);
        }

        [Fact]
        public void WithDirectives()
        {
            /* Given */
            ScalarDefinition original = @"scalar Name";

            /* When */
            var modified = original
                .WithDirectives(new List<Directive>
                {
                    "@a"
                });

            /* Then */
            Assert.Null(original.Directives);
            Assert.NotNull(modified.Directives);
            var a = Assert.Single(modified.Directives);
            Assert.Equal("a", a?.Name);
        }
    }
}