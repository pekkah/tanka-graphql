using Tanka.GraphQL.Execution;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

public class NodePathFacts
{
    [Fact]
    public void Append_fieldName_segment()
    {
        /* Given */
        var sut = new NodePath();

        /* When */
        sut.Append("fieldName");

        /* Then */
        Assert.Contains("fieldName", sut.Segments);
    }

    [Fact]
    public void Append_index_segment()
    {
        /* Given */
        var sut = new NodePath();

        /* When */
        sut.Append(0);

        /* Then */
        Assert.Contains(0, sut.Segments);
    }

    [Fact]
    public void Fork_matches_original()
    {
        /* Given */
        var sut = new NodePath();
        sut.Append("humans");
        sut.Append(0);
        sut.Append("name");

        /* When */
        var fork = sut.Fork();

        /* Then */
        Assert.Equal(sut.Segments, fork.Segments);
    }

    [Fact]
    public void Fork_is_separate()
    {
        /* Given */
        var sut = new NodePath();
        sut.Append("humans");
        sut.Append(0);
        sut.Append("name");
        var fork = sut.Fork();

        /* When */
        fork.Append(0);

        /* Then */
        Assert.NotEqual(sut.Segments, fork.Segments);
    }
}