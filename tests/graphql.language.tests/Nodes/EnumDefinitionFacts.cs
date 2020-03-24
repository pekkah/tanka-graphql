using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
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

        [Fact]
        public void WithDescription()
        {
            /* Given */
            EnumDefinition original = @"enum ENUM { V1, V2 }";

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
            EnumDefinition original = @"enum ENUM { V1, V2 }";

            /* When */
            var modified = original
                .WithName("Renamed");

            /* Then */
            Assert.Equal("ENUM", original.Name);
            Assert.Equal("Renamed", modified.Name);
        }

        [Fact]
        public void WithValues()
        {
            /* Given */
            EnumDefinition original = @"enum ENUM { V1, V2 }";

            /* When */
            var modified = original
                .WithValues(new List<EnumValueDefinition>
                {
                    "V3 @new"
                });

            /* Then */
            Assert.Equal(2, original.Values?.Count);
            Assert.Equal(1, modified.Values?.Count);
        }

        [Fact]
        public void WithDirectives()
        {
            /* Given */
            EnumDefinition original = @"enum ENUM { V1, V2 }";

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