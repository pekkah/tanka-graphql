using System.Collections.Generic;
using Tanka.GraphQL.Fields;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class ArgumentBinderFacts
{
    [Fact]
    public void BindArgument()
    {
        /* Given */

        var test = new Test();
        var inputObjectValue = new Dictionary<string, object?>
        {
            ["StringProp"] = "test"
        };

        /* When */
        ArgumentBinderFeature.BindInputObject(inputObjectValue, test);

        /* Then */
        Assert.Equal("test", test.StringProp);
    }

    private class Test
    {
        public string StringProp { get; set; }
    }
}