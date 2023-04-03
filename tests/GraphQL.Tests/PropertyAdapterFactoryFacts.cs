using Tanka.GraphQL.Internal;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class PropertyAdapterFactoryFacts
{
    [Fact]
    public void SetValue()
    {
        /* Given */
        var test = new Test();
        var properties = PropertyAdapterFactory.GetPropertyAdapters<Test>();
        var testAdapter = properties["StringProp"];
        
        /* When */
        testAdapter.SetValue(test, "test");

        /* Then */
        Assert.Equal("test", test.StringProp);
    }

    [Fact]
    public void GetValue()
    {
        /* Given */
        var test = new Test()
        {
            StringProp = "test"
        };
        var properties = PropertyAdapterFactory.GetPropertyAdapters<Test>();
        var testAdapter = properties["StringProp"];

        /* When */
        var actual = testAdapter.GetValue(test);

        /* Then */
        Assert.Equal("test", actual);
    }

    private class Test
    {
        public string StringProp { get; set; }
    }
}