﻿using System.Linq;
using System.Text;

using Tanka.GraphQL.Language.Nodes.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class SchemaExtensionFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        SchemaExtension original = "extend schema { query: Query }"u8;

        /* Then */
        Assert.Equal("Query", original.Operations?.Single().NamedType.Name);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        SchemaExtension original = "extend schema { query: Query }";

        /* Then */
        Assert.Equal("Query", original.Operations?.Single().NamedType.Name);
    }
}