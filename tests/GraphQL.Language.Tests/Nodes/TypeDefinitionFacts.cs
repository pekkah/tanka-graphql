﻿using System.Text;

using Tanka.GraphQL.Language.Nodes.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class TypeDefinitionFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        TypeDefinition original = "enum Enum"u8;

        /* Then */
        Assert.IsType<EnumDefinition>(original);
    }

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