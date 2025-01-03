﻿using System.Text;

using Tanka.GraphQL.Language.Nodes;

using Xunit;

namespace Tanka.GraphQL.Language.Tests.Nodes;

public class ExecutableDocumentFacts
{
    [Fact]
    public void FromBytes()
    {
        /* Given */
        /* When */
        ExecutableDocument original = "{ field1 field2 }"u8;

        /* Then */
        Assert.NotNull(original.OperationDefinitions);
        Assert.Single(original.OperationDefinitions);
    }

    [Fact]
    public void FromString()
    {
        /* Given */
        /* When */
        ExecutableDocument original = "{ field1 field2 }";

        /* Then */
        Assert.NotNull(original.OperationDefinitions);
        Assert.Single(original.OperationDefinitions);
    }
}
