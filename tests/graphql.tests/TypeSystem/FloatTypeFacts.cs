
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class FloatTypeFacts
    {
        private readonly IValueConverter _sut;

        public FloatTypeFacts()
        {
            _sut = new DoubleConverter();
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData(123.123, 123.123)]
        [InlineData("123", 123)]
        [InlineData("123.123", 123.123)]
        public void ParseValue(object input, double expected)
        {
            /* Given */
            /* When */
            var actual = _sut.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        [InlineData("123.123", 123.123)]
        public void ParseLiteral(string input, double expected)
        {
            /* Given */
            Value astValue = input;

            /* When */
            var actual = _sut.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        public void ParseIntLiteral(string input, double expected)
        {
            /* Given */
            Value astValue = input;

            /* When */
            var actual = _sut.ParseLiteral(astValue);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData(123.123, 123.123)]
        [InlineData("123", 123)]
        [InlineData("123.123", 123.123)]
        public void Serialize(object input, double expected)
        {
            /* Given */
            /* When */
            var actual = _sut.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}