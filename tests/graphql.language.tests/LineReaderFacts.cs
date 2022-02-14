using System.Text;
using Tanka.GraphQL.Language.Internal;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

public class LineReaderFacts
{
    [Fact]
    public void ReadLines()
    {
        /* Given */
        var source = "first\nsecond";

        var sut = new LineReader(Encoding.UTF8.GetBytes(source));

        /* When */
        Assert.True(sut.TryReadLine(out var firstLine));
        Assert.True(sut.TryReadLine(out var secondLine));
        Assert.False(sut.TryReadLine(out _));

        /* Then */
        Assert.Equal("first", Encoding.UTF8.GetString(firstLine));
        Assert.Equal("second", Encoding.UTF8.GetString(secondLine));
    }
}