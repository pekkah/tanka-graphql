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

    [Fact]
    public void BindArgument_Complex()
    {
        /* Given */

        var complex = new Complex();
        var inputObjectValue = new Dictionary<string, object?>
        {
            ["Child"] = new Dictionary<string, object?>
            {
                ["StringProp"] = "test"
            }
        };

        /* When */
        ArgumentBinderFeature.BindInputObject(inputObjectValue, complex);

        /* Then */
        Assert.Equal("test", complex.Child.StringProp);
    }

    private class Test
    {
        public string StringProp { get; set; }
    }

    private class Complex : IParseableInputObject
    {
        public ComplexChild? Child { get; } = new ComplexChild();

        public void Parse(IReadOnlyDictionary<string, object?> argumentValue)
        {
            if (argumentValue.TryGetValue("Child", out var value))
                ArgumentBinderFeature.BindInputObject(((IReadOnlyDictionary<string, object?>)value)!, Child);
        }
    }

    private class ComplexChild : IParseableInputObject
    {
        public string? StringProp { get; set; }

        public void Parse(IReadOnlyDictionary<string, object?> argumentValue)
        {
            if (argumentValue.TryGetValue("StringProp", out var value))
                StringProp = value as string;
        }
    }
}