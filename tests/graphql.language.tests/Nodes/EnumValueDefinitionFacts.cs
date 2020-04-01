﻿using System.Collections.Generic;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class EnumValueDefinitionFacts
    {
        [Fact]
        public void FromBytes()
        {
            /* Given */
            /* When */
            EnumValueDefinition original = Encoding.UTF8.GetBytes("V1")
                .AsReadOnlySpan();

            /* Then */
            Assert.Equal("V1", original.Value.Name);
        }
        
        [Fact]
        public void FromString()
        {
            /* Given */
            /* When */
            EnumValueDefinition original = "V1";

            /* Then */
            Assert.Equal("V1", original.Value.Name);
        }

        [Fact]
        public void WithDescription()
        {
            /* Given */
            EnumValueDefinition original = @"V1";

            /* When */
            var modified = original
                .WithDescription("Description");

            /* Then */
            Assert.Null(original.Description);
            Assert.Equal("Description", modified.Description);
        }

        [Fact]
        public void WithValue()
        {
            /* Given */
            EnumValueDefinition original = @"V1";

            /* When */
            var modified = original
                .WithValue(new EnumValue("RENAMED"));

            /* Then */
            Assert.Equal("V1", original.Value.Name);
            Assert.Equal("RENAMED", modified.Value.Name);
        }

        [Fact]
        public void WithDirectives()
        {
            /* Given */
            EnumValueDefinition original = @"V1";

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