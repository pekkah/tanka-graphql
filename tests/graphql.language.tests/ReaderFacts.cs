using System.Text;
using Xunit;

namespace Tanka.GraphQL.Language.Tests
{
    public class ReaderFacts
    {
        [Theory]
        [InlineData(0, 0, '0')]
        [InlineData(1, 1, '1')]
        [InlineData(2, 2, '2')]
        [InlineData(9, 9, '9')]
        public void Advance(
            int count,
            int expectedPosition,
            byte expectedCurrent)
        {
            /* Given */
            var data = "0123456789";
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            Assert.True(sut.Advance(count));
            Assert.True(sut.TryRead(out var current));

            /* Then */
            Assert.Equal(expectedCurrent, current);
            Assert.Equal(expectedPosition, sut.Position);
        }

        [Fact]
        public void Empty()
        {
            /* Given */
            var data = string.Empty;
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            /* Then */
            Assert.False(sut.TryPeek(out _));
            Assert.False(sut.Advance());
            Assert.Equal(0, sut.Position);
        }

        [Fact]
        public void TryRead_False()
        {
            /* Given */
            var data = "123";
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            /* Then */
            Assert.True(sut.TryRead(out _));
            Assert.True(sut.TryRead(out _));
            Assert.True(sut.TryRead(out _));
            Assert.False(sut.TryRead(out _));
        }

        [Fact]
        public void TryPeek_False()
        {
            /* Given */
            var data = "123";
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            /* Then */
            Assert.True(sut.TryRead(out _));
            Assert.True(sut.TryRead(out _));
            Assert.True(sut.TryRead(out _));
            Assert.False(sut.TryPeek(out _));
        }

        [Fact]
        public void Advance_False()
        {
            /* Given */
            var data = "123";
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            /* Then */
            Assert.True(sut.Advance());
            Assert.True(sut.Advance());
            Assert.True(sut.Advance());
            Assert.False(sut.Advance());
        }

        [Fact]
        public void With_data()
        {
            /* Given */
            var data = "0123456789";
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            /* Then */
            Assert.Equal(-1, sut.Position);
            Assert.Equal(data.Length, sut.Length);
            Assert.True(sut.TryPeek(out _));
            Assert.True(sut.Advance());
        }

        [Theory]
        [InlineData("test", "test", true)]
        [InlineData("test", "exp", false)]
        public void IsNext(string data, string expectedNext, bool expectedIsNext)
        {
            /* Given */
            var sut = new Reader(Encoding.UTF8.GetBytes(data));

            /* When */
            var actualIsNext = sut.IsNext(Encoding.UTF8.GetBytes(expectedNext));

            /* Then */
            Assert.Equal(expectedIsNext, actualIsNext);
        }
    }
}