
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem
{
    public class IntTypeFacts
    {
        private readonly IValueConverter _sut;

        public IntTypeFacts()
        {
            _sut = new IntConverter();
        }

        [Theory]
        [InlineData(123, 123)]
        [InlineData("123", 123)]
        public void ParseValue(object input, int expected)
        {
            /* Given */
            /* When */
            var actual = _sut.ParseValue(input);

            /* Then */
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("123", 123)]
        public void ParseLiteral(string input, int expected)
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
        [InlineData("123", 123)]
        public void Serialize(object input, int expected)
        {
            /* Given */
            /* When */
            var actual = _sut.Serialize(input);

            /* Then */
            Assert.Equal(expected, actual);
        }
    }
}