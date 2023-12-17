using System;
using System.Text;
using Tanka.GraphQL.Language.Internal;
using Xunit;

namespace Tanka.GraphQL.Language.Tests;

public class BufferWriterFacts
{
    [Fact]
    public void Create()
    {
        /* Given */
        using var sut = new BufferWriter(1024);

        /* Then */
        Assert.Equal(1024, sut.FreeSpan.Length);
        Assert.Equal(0, sut.WrittenSpan.Length);
    }

    [Fact]
    public void Write()
    {
        /* Given */
        ReadOnlySpan<byte> data = "12345"u8;

        const int bufferSize = 1024;
        using var sut = new BufferWriter(bufferSize);

        /* When */
        sut.Write(data);

        /* Then */
        Assert.Equal(bufferSize - data.Length, sut.FreeSpan.Length);
        Assert.Equal(data.Length, sut.WrittenSpan.Length);
        Assert.Equal("12345", Encoding.UTF8.GetString(sut.WrittenSpan));
    }

    [Fact]
    public void Write_too_much()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ReadOnlySpan<byte> data = "12345"u8;
            const int bufferSize = 4;
            using var sut = new BufferWriter(bufferSize);
            sut.Write(data);
        });
    }
}