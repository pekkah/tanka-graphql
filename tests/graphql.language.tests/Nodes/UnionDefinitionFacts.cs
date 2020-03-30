﻿using System.Collections.Generic;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class UnionDefinitionFacts
    {
        [Fact]
        public void FromBytes()
        {
            /* Given */
            /* When */
            UnionDefinition original = Encoding.UTF8.GetBytes("union Name = MemberA | MemberB")
                .AsReadOnlySpan();

            /* Then */
            Assert.Equal("Name", original.Name);
            Assert.Equal(2, original.Members?.Count);
        }
        
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

        [Fact]
        public void WithDescription()
        {
            /* Given */
            UnionDefinition original = "union Name = MemberA | MemberB";

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
            UnionDefinition original = "union Name = MemberA | MemberB";

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
            UnionDefinition original = "union Name = MemberA | MemberB";

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

        [Fact]
        public void WithMembers()
        {
            /* Given */
            UnionDefinition original = "union Name = MemberA | MemberB";

            /* When */
            var modified = original
                .WithMembers(new List<NamedType>
                {
                    "a"
                });

            /* Then */
            Assert.NotNull(modified.Members);
            var a = Assert.Single(modified.Members);
            Assert.Equal("a", a?.Name);
        }
    }
}