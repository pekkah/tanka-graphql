﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes
{
    public class DirectiveFacts
    {
        [Fact]
        public void FromBytes()
        {
            /* Given */
            /* When */
            Directive original = Encoding.UTF8.GetBytes("@a(x: 100, y: 100)")
                .AsReadOnlySpan();

            /* Then */
            Assert.Equal("a", original.Name);
            Assert.NotNull(original.Arguments);
            Assert.Equal(2, original.Arguments?.Count);
        }
        
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

        [Fact]
        public void WithArguments()
        {
            /* Given */
            Directive original = "@a(x: 100, y: 100)";

            /* When */
            var modified = original
                .WithArguments(
                    new List<Argument>(original.Arguments ?? Enumerable.Empty<Argument>())
                        .Concat(new []
                        {
                            new Argument("x", new IntValue(100, default), default), 
                        }).ToList()
                    );

            /* Then */
            Assert.Equal(2, original.Arguments?.Count);
            Assert.Equal(3, modified.Arguments?.Count);
        }

        [Fact]
        public void WithName()
        {
            /* Given */
            Directive original = "@a(x: 100, y: 100)";

            /* When */
            var modified = original
                .WithName("b");

            /* Then */
            Assert.Equal("a", original.Name);
            Assert.Equal("b", modified.Name);
        }
    }
}